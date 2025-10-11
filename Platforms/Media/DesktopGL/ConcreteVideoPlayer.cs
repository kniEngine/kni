// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private VideoDecoderProcess _decoderProcess;
        private Texture2D _lastFrame;

        private DynamicSoundEffectInstance _soundPlayer;

        private Stopwatch _watch = new Stopwatch();
        private Thread _decoderThread;
        DecoderThreadState _decoderThreadState;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                PlatformSetVolume();
            }
        }

        public override TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                PlatformSetVolume();
            }
        }

        internal ConcreteVideoPlayerStrategy()
        {

        }

        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != base.Video.Width || _lastFrame.Height != base.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(((IPlatformVideo)base.Video).Strategy.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Color);

            if (_decoderProcess._isVideoFrameDataDirty)
            {
                _lastFrame.SetData(_decoderProcess._videoFrameData);
                _decoderProcess._isVideoFrameDataDirty = false;
            }

            return _lastFrame;
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            
        }

        private class DecoderThreadState
        {
            public readonly VideoDecoderProcess DecoderProcess;
            public MediaState State;

            public DecoderThreadState(VideoDecoderProcess decoderProcess, MediaState playing)
            {
                this.DecoderProcess = decoderProcess;
                this.State = playing;
            }
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            // Cleanup the last video first.
            if (State != MediaState.Stopped)
            {
                _decoderProcess.Dispose();
                _decoderProcess = null;
            }

            string ffmpegPath = "x64\\ffmpeg";
            string inputFile = "" + ((IPlatformVideo)base.Video).Strategy.FileName;

            _decoderProcess = new VideoDecoderProcess(base.Video, ffmpegPath, inputFile);

            _decoderProcess.ParseMKV();

            VideoDecoderProcess.MKVTrack atrack = _decoderProcess._tracks.Values.First( (t) => (t.Type == VideoDecoderProcess.MKVTrackType.Audio) );
            VideoDecoderProcess.MKVAudioTrack audioTrack = (VideoDecoderProcess.MKVAudioTrack)atrack;
            VideoDecoderProcess.MKVAudioSettings audioSettings = audioTrack.AudioSettings;

            VideoDecoderProcess.MKVTrack vtrack = _decoderProcess._tracks.Values.First( (t) => (t.Type == VideoDecoderProcess.MKVTrackType.Video) );
            VideoDecoderProcess.MKVVideoTrack videoTrack = (VideoDecoderProcess.MKVVideoTrack)vtrack;
            VideoDecoderProcess.MKVVideoSettings videoSettings = videoTrack.VideoSettings;

            int SampleRate = (int)audioSettings.SamplingFrequency;
            AudioChannels channels = (AudioChannels)audioSettings.Channels;
            _soundPlayer = new DynamicSoundEffectInstance(SampleRate, channels);
            int samples = (SampleRate * (int)channels) / 2;
            PlatformSetVolume();
            _soundPlayer.Play();

            _watch.Reset();
            _watch.Start();
            _decoderThread = new Thread(OnUpdate);
            _decoderThread.IsBackground = true;
            DecoderThreadState args = new DecoderThreadState(_decoderProcess, MediaState.Playing);
            _decoderThread.Start(args);
        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformResume()
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            if (_decoderThread != null)
            {
                _decoderThreadState.State = MediaState.Stopped;
                _decoderThreadState = null;
                _decoderThread = null;
            }
            _watch.Stop();
            _soundPlayer.Stop();
            _soundPlayer.Dispose();
            _soundPlayer = null;
        }

        private void PlatformSetVolume()
        {
           float volume = base.Volume;
           if (IsMuted)
                volume = 0.0f;

           if (_soundPlayer != null)
              _soundPlayer.Volume = volume;
        }

        private void OnUpdate(object obj)
        {
            DecoderThreadState args = (DecoderThreadState)obj;

            while (args.State == MediaState.Playing)
            {
                while (!_decoderProcess._isVideoFrameDataDirty)
                {
                    _decoderProcess.ParseSegmentCluster2(_decoderProcess._binaryReader, (long)0);

                    // Push audio buffer if available
                    if (_decoderProcess._audioFrameData != null && _decoderProcess._audioFrameData.Length > 0)
                    {
                        _soundPlayer.SubmitBuffer(_decoderProcess._audioFrameData, 0, _decoderProcess._audioFrameData.Length);
                        _decoderProcess._audioFrameData = null;
                    }
                }

                int duration = 1000 / 30; // 30fps
                Thread.Sleep(duration);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_decoderThread != null)
                {
                    _decoderThreadState.State = MediaState.Stopped;
                    _decoderThreadState = null;
                    _decoderThread = null;
                }
                if (_decoderProcess != null)
                {
                    _decoderProcess.Dispose();
                    _decoderProcess = null;
                }
                if (_soundPlayer != null)
                {
                    _soundPlayer.Dispose();
                    _soundPlayer = null;
                }

            }

            if (_watch != null)
            {
                _watch.Stop();
                _watch = null;
            }

            base.Dispose(disposing);
        }
    }
}
