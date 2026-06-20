// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Media;

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

        internal Thread _thread;

        public event EventHandler Stopped;

        public DecoderThread(ConcreteVideoPlayerStrategy videoPlayerStrategy)
        {
            this.VideoPlayerStrategy = videoPlayerStrategy;
            this._threadState = ThreadState.None;

            string ffmpegPath = "x64\\ffmpeg";
            string inputFile = "" + ((IPlatformVideo)VideoPlayerStrategy.Video).Strategy.FileName;
            this.DecoderProcess = new VideoDecoderProcess(VideoPlayerStrategy.Video, ffmpegPath, inputFile);

            _thread = new Thread(DecoderThread.OnThreadStart);
            _thread.IsBackground = true;

            DecoderProcess.ParseMKV(_videoFrameQueue, _audioFrameQueue, _videoFramePool, _audioFramePool);
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
                                                                          decoderThread._videoFramePool, decoderThread._audioFramePool);
                    }
                    catch (EndOfStreamException eosEx)
                    {
                        if (videoPlayerStrategy.IsLooped)
                        {
                            decoderThread.Watch.Stop();

                            decoderThread.DecoderProcess.Dispose();
                            decoderThread.DecoderProcess = null;

                            string ffmpegPath = "x64\\ffmpeg";
                            string inputFile = "" + ((IPlatformVideo)videoPlayerStrategy.Video).Strategy.FileName;
                            decoderThread.DecoderProcess = new VideoDecoderProcess(videoPlayerStrategy.Video, ffmpegPath, inputFile);

                            decoderThread.DecoderProcess.ParseMKV(decoderThread._videoFrameQueue, decoderThread._audioFrameQueue,
                                                                  decoderThread._videoFramePool, decoderThread._audioFramePool);

                            decoderThread.Watch.Reset();
                            decoderThread.Watch.Start();
                        }
                        else
                        {
                            reachEndOfStream = true;
                        }
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
                        if (!videoPlayerStrategy.IsLooped)
                        {
                            decoderThread._threadState = ThreadState.Stop;
                            var handler = decoderThread.Stopped;
                            if (handler != null)
                                handler(decoderThread, EventArgs.Empty);
                            break;
                        }
                        else
                        {
                            throw new NotSupportedException("Looping is not supported.");
                        }
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
