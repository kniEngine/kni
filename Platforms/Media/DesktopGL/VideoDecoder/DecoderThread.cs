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
        public readonly ConcreteVideoPlayerStrategy VideoPlayerStrategy;
        public readonly VideoDecoderProcess DecoderProcess;
        internal readonly Stopwatch Watch = new Stopwatch();
        private volatile ThreadState _threadState;

        private VideoDecoderProcess.TrackData? _nextVideoFrame;
        private object _nextVideoFrameLock = new object();

        internal Thread _thread;

        public event EventHandler Stopped;

        public DecoderThread(ConcreteVideoPlayerStrategy videoPlayerStrategy, VideoDecoderProcess decoderProcess)
        {
            this.VideoPlayerStrategy = videoPlayerStrategy;
            this.DecoderProcess = decoderProcess;
            this._threadState = ThreadState.None;

            _thread = new Thread(DecoderThread.OnThreadStart);
            _thread.IsBackground = true;
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
            VideoDecoderProcess decoderProcess = decoderThread.DecoderProcess;

            while (true)
            {
                if (decoderThread._threadState == ThreadState.Stop)
                    break;

                if (decoderProcess._videoFrameQueue.Count < 1
                ||  decoderProcess._audioFrameQueue.Count < 1)
                {
                    try
                    {
                        decoderProcess.ParseSegmentCluster2(decoderProcess._binaryReader, (long)0);
                    }
                    catch (EndOfStreamException eosEx)
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
                            throw new NotSupportedException("Looping is not supported.", eosEx);
                        }
                    }
                }

                while (decoderProcess.TryPeekAudioFrame(out VideoDecoderProcess.TrackData audioFrame)
                   && videoPlayerStrategy._soundPlayer.PendingBufferCount <= 3)
                {
                    decoderProcess._audioFrameQueue.Dequeue();
                    videoPlayerStrategy._soundPlayer.SubmitBuffer(audioFrame.Data, 0, audioFrame.Data.Length);
                    decoderProcess._audioFramePool.Return(audioFrame.Data);
                }

                while (decoderProcess.TryPeekVideoFrame(out VideoDecoderProcess.TrackData frameData)
                   &&  frameData.TrackTime <= decoderThread.Watch.Elapsed)
                {
                    decoderProcess._videoFrameQueue.Dequeue();
                    lock (decoderThread._nextVideoFrameLock)
                    {
                        decoderThread._nextVideoFrame = frameData;
                    }
                }

                Thread.Sleep(0);
            }

            return;
        }

        internal bool TryGetNextVideoFrame(out VideoDecoderProcess.TrackData trackData)
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
                    trackData = default(VideoDecoderProcess.TrackData);
                    return false;
                }
            }
        }

        public void Stop()
        {
            _threadState = ThreadState.Stop;
            _thread.Join();
            _thread = null;

            Watch.Stop();
        }
    }
}
