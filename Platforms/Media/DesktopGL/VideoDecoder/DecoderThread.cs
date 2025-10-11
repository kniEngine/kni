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
            DecoderThread args = (DecoderThread)obj;
            ConcreteVideoPlayerStrategy videoPlayerStrategy = args.VideoPlayerStrategy;
            VideoDecoderProcess decoderProcess = args.DecoderProcess;

            while (true)
            {
                if (args._threadState == ThreadState.Stop)
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
                            args._threadState = ThreadState.Stop;
                            var handler = args.Stopped;
                            if (handler != null)
                                handler(args, EventArgs.Empty);
                            break;
                        }
                        else
                        {
                            throw new NotSupportedException("Looping is not supported.", eosEx);
                        }
                    }

                    while (videoPlayerStrategy._soundPlayer.PendingBufferCount <= 3 
                       &&  decoderProcess.TryPeekAudioFrame(out VideoDecoderProcess.TrackData audioFrame))
                    {
                        videoPlayerStrategy._soundPlayer.SubmitBuffer(audioFrame.Data, 0, audioFrame.Data.Length);
                        decoderProcess._audioFrameQueue.Dequeue();
                        decoderProcess._audioFramePool.Return(audioFrame.Data);
                    }
                }

                Thread.Sleep(0);
            }

            return;
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
