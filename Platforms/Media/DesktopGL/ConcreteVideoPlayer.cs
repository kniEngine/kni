// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private VideoDecoderProcess _decoderProcess;
        private Texture2D _lastFrame;

        private DynamicSoundEffectInstance _soundPlayer;

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
                throw new NotImplementedException();
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
                if (base.Video != null)
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

            // test. read next frame.
            if (false)
            {
                while (!_decoderProcess._isVideoFrameDataDirty)
                    _decoderProcess.ParseSegmentCluster2(_decoderProcess._binaryReader, (long)0);
            }

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

            VideoDecoderProcess.MKVTrack track = _decoderProcess._tracks.Values.First( (t) => (t.Type == VideoDecoderProcess.MKVTrackType.Audio) );
            VideoDecoderProcess.MKVAudioTrack audioTrack = (VideoDecoderProcess.MKVAudioTrack)track;
            VideoDecoderProcess.MKVAudioSettings audioSettings = audioTrack.AudioSettings;
            int SampleRate = (int)audioSettings.SamplingFrequency;
            AudioChannels channels = (AudioChannels)audioSettings.Channels;
            _soundPlayer = new DynamicSoundEffectInstance(SampleRate, channels);
            int samples = (SampleRate * (int)channels) / 2;

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
            throw new NotImplementedException();
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_decoderProcess != null)
                {
                    _decoderProcess.Dispose();
                    _decoderProcess = null;
                }

            }


            base.Dispose(disposing);
        }
    }
}
