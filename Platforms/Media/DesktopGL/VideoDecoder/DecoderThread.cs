// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Platform.Media
{
    internal enum ThreadState
    {
        None,
        Playing,
        Stop,
        Suspend,
    }

    internal class DecoderThread
    {
        private readonly ConcreteVideoPlayerStrategy VideoPlayerStrategy;
        public VideoDecoderProcess DecoderProcess;
        internal readonly Stopwatch Watch = new Stopwatch();
        private volatile ThreadState _threadState;

        internal Queue<TrackData> _videoFrameQueue = new Queue<TrackData>();
        internal Queue<TrackData> _audioFrameQueue = new Queue<TrackData>();
        internal FrameBufferPool _videoFramePool = new FrameBufferPool();
        internal FrameBufferPool _audioFramePool = new FrameBufferPool();

        private TrackData? _nextVideoFrame;
        private object _nextVideoFrameLock = new object();
        private int _currentLoopCount = 0;
        private int _prevLoopCount = 0;

        internal Thread _thread;

        public event EventHandler Stopped;

        public DecoderThread(ConcreteVideoPlayerStrategy videoPlayerStrategy)
        {
            this.VideoPlayerStrategy = videoPlayerStrategy;
            this._threadState = ThreadState.None;

            string ffmpegPath = GetFFmpegPath();
            string inputFile = "" + ((IPlatformVideo)VideoPlayerStrategy.Video).Strategy.FileName;
            this.DecoderProcess = new VideoDecoderProcess(VideoPlayerStrategy.Video, ffmpegPath, inputFile);

            _thread = new Thread(DecoderThread.OnThreadStart);
            _thread.IsBackground = true;

            DecoderProcess.ParseMKV(_videoFrameQueue, _audioFrameQueue, _videoFramePool, _audioFramePool, _currentLoopCount);
        }

        public void Start()
        {
            this._threadState = ThreadState.Playing;
            _thread.Start(this);

            Watch.Reset();
            Watch.Start();
        }

        private static void OnThreadStart(object obj)
        {
            DecoderThread decoderThread = (DecoderThread)obj;
            ConcreteVideoPlayerStrategy videoPlayerStrategy = decoderThread.VideoPlayerStrategy;

            bool reachEndOfStream = false;
            while (true)
            {
                if (decoderThread._threadState == ThreadState.Stop)
                    break;

                if (decoderThread._videoFrameQueue.Count < 1
                ||  decoderThread._audioFrameQueue.Count < 1)
                {
                    try
                    {
                        decoderThread.DecoderProcess.ParseSegmentCluster2(decoderThread.DecoderProcess._binaryReader, (long)0,
                                                                          decoderThread._videoFrameQueue, decoderThread._audioFrameQueue,
                                                                          decoderThread._videoFramePool, decoderThread._audioFramePool, 
                                                                          decoderThread._currentLoopCount);
                    }
                    catch (EndOfStreamException eosEx)
                    {
                        if (videoPlayerStrategy.IsLooped)
                        {
                            decoderThread._currentLoopCount++;

                            decoderThread.DecoderProcess.Dispose();
                            decoderThread.DecoderProcess = null;

                            string ffmpegPath = GetFFmpegPath();
                            string inputFile = "" + ((IPlatformVideo)videoPlayerStrategy.Video).Strategy.FileName;
                            decoderThread.DecoderProcess = new VideoDecoderProcess(videoPlayerStrategy.Video, ffmpegPath, inputFile);

                            decoderThread.DecoderProcess.ParseMKV(decoderThread._videoFrameQueue, decoderThread._audioFrameQueue,
                                                                  decoderThread._videoFramePool, decoderThread._audioFramePool,
                                                                  decoderThread._currentLoopCount);
                        }
                        else
                        {
                            reachEndOfStream = true;
                        }
                    }
                }

                lock (AudioService.SyncHandle)
                {
                    if (videoPlayerStrategy._soundPlayer != null)
                    {
                        ConcreteDynamicSoundEffectInstance cdsei = ((IPlatformSoundEffectInstance)videoPlayerStrategy._soundPlayer)
                            .GetStrategy<ConcreteDynamicSoundEffectInstance>();
                        cdsei.DynamicPlatformUpdateBuffers();
                    }
                }

                while (decoderThread.TryPeekAudioFrame(out TrackData audioFrame)
                   && videoPlayerStrategy._soundPlayer.PendingBufferCount <= 3)
                {
                    decoderThread._audioFrameQueue.Dequeue();
                    videoPlayerStrategy._soundPlayer.SubmitBuffer(audioFrame.Data, 0, audioFrame.Data.Length);
                    decoderThread._audioFramePool.Return(audioFrame.Data);
                }

                while (decoderThread.TryPeekVideoFrame(out TrackData frameData)
                   &&  frameData.TrackTime <= decoderThread.Watch.Elapsed)
                {
                    if (frameData.LoopCount > decoderThread._prevLoopCount)
                    {
                        decoderThread.Watch.Restart();
                        decoderThread._prevLoopCount = frameData.LoopCount;
                    }

                    decoderThread._videoFrameQueue.Dequeue();
                    lock (decoderThread._nextVideoFrameLock)
                    {
                        decoderThread._nextVideoFrame = frameData;
                    }
                }

                if (reachEndOfStream)
                {
                    if (decoderThread._videoFrameQueue.Count < 1
                    &&  decoderThread._audioFrameQueue.Count < 1)
                    {
                        Debug.Assert(videoPlayerStrategy.IsLooped == false);

                        decoderThread._threadState = ThreadState.Stop;
                        var handler = decoderThread.Stopped;
                        if (handler != null)
                            handler(decoderThread, EventArgs.Empty);

                        decoderThread._threadState = ThreadState.Stop;
                        decoderThread.Watch.Stop();
                        decoderThread.DecoderProcess.Dispose();
                        decoderThread.DecoderProcess = null;

                        return;
                    }
                }

                Thread.Sleep(0);
            }

            return;
        }

        internal bool TryGetNextVideoFrame(out TrackData trackData)
        {
            lock (_nextVideoFrameLock)
            {
                if (_nextVideoFrame != null
                &&  _nextVideoFrame.Value.TrackTime <= Watch.Elapsed)
                {
                    trackData = _nextVideoFrame.Value;
                    _nextVideoFrame = null;
                    return true;
                }
                else
                {
                    trackData = default(TrackData);
                    return false;
                }
            }
        }

        public bool TryPeekVideoFrame(out TrackData frameData)
        {
            if (_videoFrameQueue.Count > 0)
            {
                frameData = _videoFrameQueue.Peek();
                return true;
            }
            frameData = default(TrackData);
            return false;
        }

        public bool TryPeekAudioFrame(out TrackData frameData)
        {
            if (_audioFrameQueue.Count > 0)
            {
                frameData = _audioFrameQueue.Peek();
                return true;
            }
            frameData = default(TrackData);
            return false;
        }

        private static string GetFFmpegPath()
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                    {
                        string arch = Environment.Is64BitProcess ? "x64" : "x86";
                        string ffmpegPath = Path.Combine(arch, "ffmpeg.exe");
                        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ffmpegPath)))
                            return ffmpegPath;

                        return "ffmpeg.exe";
                    }
                default:
                    return "ffmpeg";
            }
        }

        public void Stop()
        {
            _threadState = ThreadState.Stop;
            _thread.Join();
            _thread = null;

            Watch.Stop();

            DecoderProcess.Dispose();
            DecoderProcess = null;
        }
    }
}
