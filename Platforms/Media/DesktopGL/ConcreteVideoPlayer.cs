// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private VideoDecoderProcess _decoderProcess;
        private Texture2D[] _textures = new Texture2D[2];
        private int _currentTexture;
        private TimeSpan _lastFrameTime;

        internal DynamicSoundEffectInstance _soundPlayer;

        DecoderThread _decoderThread;

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
            get { return _decoderThread.Watch.Elapsed; }
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
            if (_textures[0] != null)
            {
                if (_textures[0].Width != base.Video.Width || _textures[0].Height != base.Video.Height)
                {
                    _textures[0].Dispose();
                    _textures[0] = null;
                    _textures[1].Dispose();
                    _textures[1] = null;
                }
            }
            if (_textures[0] == null)
            {
                GraphicsDevice graphicsDevice = ((IPlatformVideo)base.Video).Strategy.GraphicsDevice;
                _textures[0] = new Texture2D(graphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Color);
                _textures[1] = new Texture2D(graphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Color);
            }

            if (_decoderProcess != null)
            {
                if (_decoderThread.TryGetNextVideoFrame(out VideoDecoderProcess.TrackData frameData))
                {
                    _lastFrameTime = frameData.TrackTime;
                    _currentTexture = (_currentTexture+1) & 0x01;
                    _textures[_currentTexture].SetData(frameData.Data);
                    _decoderProcess._videoFramePool.Return(frameData.Data);
                }
            }

            return _textures[_currentTexture];
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            // Cleanup the last video first.
            if (State != MediaState.Stopped)
            {
                _decoderThread.Stop();
                _decoderThread = null;
                _decoderProcess.Dispose();
                _decoderProcess = null;
                _soundPlayer.Dispose();
                _soundPlayer = null;
            }

            string ffmpegPath = "x64\\ffmpeg";
            string inputFile = "" + ((IPlatformVideo)base.Video).Strategy.FileName;

            _decoderProcess = new VideoDecoderProcess(base.Video, ffmpegPath, inputFile);

            _decoderProcess.ParseMKV();

            VideoDecoderProcess.MKVTrack atrack = _decoderProcess._tracks.Values.First((t) => (t.Type == VideoDecoderProcess.MKVTrackType.Audio));
            VideoDecoderProcess.MKVAudioTrack audioTrack = (VideoDecoderProcess.MKVAudioTrack)atrack;
            VideoDecoderProcess.MKVAudioSettings audioSettings = audioTrack.AudioSettings;

            VideoDecoderProcess.MKVTrack vtrack = _decoderProcess._tracks.Values.First((t) => (t.Type == VideoDecoderProcess.MKVTrackType.Video));
            VideoDecoderProcess.MKVVideoTrack videoTrack = (VideoDecoderProcess.MKVVideoTrack)vtrack;
            VideoDecoderProcess.MKVVideoSettings videoSettings = videoTrack.VideoSettings;

            int SampleRate = (int)audioSettings.SamplingFrequency;
            AudioChannels channels = (AudioChannels)audioSettings.Channels;
            _soundPlayer = new DynamicSoundEffectInstance(SampleRate, channels);
            _soundPlayer.BufferNeeded += (s, e) =>
            {
                Debug.WriteLine("_soundPlayer.PendingBufferCount: " + _soundPlayer.PendingBufferCount);
                Debug.WriteLine("_videoFrameQueue.Count: " + _decoderProcess._videoFrameQueue.Count);
            };
            int samples = (SampleRate * (int)channels) / 2;
            PlatformSetVolume();
            _soundPlayer.Play();

            _lastFrameTime = TimeSpan.Zero;
            this.State = MediaState.Playing;

            _decoderThread = new DecoderThread(this, _decoderProcess);
            _decoderThread.Stopped += OnDecoderThreadStopped;
            _decoderThread.Start();
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
                _decoderThread.Stop();
                _decoderThread = null;
            }
            if (_decoderProcess != null)
            {
                _decoderProcess.Dispose();
                _decoderProcess = null;
            }

            State = MediaState.Stopped;
            _soundPlayer.Stop();
            _soundPlayer.Dispose();
            _soundPlayer = null;
        }

        private void OnDecoderThreadStopped(object sender, EventArgs e)
        {
            // TODO: event comes from another thread, so we need to sync base.State, and clean up. 
            base.State = MediaState.Stopped;

            _decoderThread.Stop();
            _decoderThread = null;
            _decoderProcess.Dispose();
            _decoderProcess = null;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_decoderThread != null)
                {
                    _decoderThread.Stop();
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
                if (_textures != null)
                {
                    if (_textures[0] != null)
                        _textures[0].Dispose();
                    if (_textures[1] != null)
                        _textures[1].Dispose();
                    _textures[0] = null;
                    _textures[1] = null;
                    _textures = null;
                }

            }


            base.Dispose(disposing);
        }
    }
}
