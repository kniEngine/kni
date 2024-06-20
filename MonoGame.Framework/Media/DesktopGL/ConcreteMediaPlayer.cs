// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using NVorbis;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        MediaPlatformStream _mediaPlatformStream;

        internal ConcreteMediaPlayerStrategy()
        {
            this._mediaPlatformStream = new MediaPlatformStream();
        }

        #region Properties

        public override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                if (base.Queue.Count > 0)
                    SetChannelVolumes();
            }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get
            {
                Song activeSong = base.Queue.ActiveSong;
                if (activeSong == null)
                    return TimeSpan.Zero;

                if (_mediaPlatformStream._reader != null)
                    return _mediaPlatformStream._reader.DecodedTime;
                else
                    return TimeSpan.Zero;
            }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        public override float PlatformVolume
        {
            set
            {
                base.PlatformVolume = value;

                if (base.Queue.ActiveSong != null)
                    SetChannelVolumes();
            }
        }

        public override bool PlatformGameHasControl
        {
            get { return true; }
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

            if (_mediaPlatformStream._player != null)
                _mediaPlatformStream._player.Volume = innerVolume;
        }

        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                _mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                if (_mediaPlatformStream._player != null)
                    _mediaPlatformStream._player.Volume = innerVolume;

                this.CreatePlayer(_mediaPlatformStream, ((IPlatformSong)song).Strategy);

                SoundState state = _mediaPlatformStream._player.State;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Paused:
                        _mediaPlatformStream._player.Resume();
                        return;
                    case SoundState.Stopped:
                        _mediaPlatformStream._player.Volume = innerVolume;
                        _mediaPlatformStream._player.Play();
                        ((IPlatformSong)song).Strategy.PlayCount++;
                        return;
                }
            }

        }

        public override void PlatformPause()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                if (_mediaPlatformStream._player != null)
                    _mediaPlatformStream._player.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                if (_mediaPlatformStream._player != null)
                    _mediaPlatformStream._player.Resume();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;

                if (_mediaPlatformStream._player != null)
                {
                    _mediaPlatformStream._player.Stop();
                    this.DestroyPlayer(_mediaPlatformStream);
                }
            }
        }

        protected override void PlatformClearQueue()
        {
            while (base.Queue.Count > 0)
            {
                Song song = base.Queue[0];

                if (_mediaPlatformStream._player != null)
                {
                    _mediaPlatformStream._player.Stop();
                    this.DestroyPlayer(_mediaPlatformStream);
                }

                base.RemoveQueuedSong(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }


        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mediaPlatformStream != null)
                {
                    _mediaPlatformStream.Dispose();
                    _mediaPlatformStream = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion


        internal delegate void FinishedPlayingHandler();

        internal void CreatePlayer(MediaPlatformStream mediaPlatformStream, SongStrategy strategy)
        {
            if (mediaPlatformStream._player == null)
            {
                mediaPlatformStream._reader = new VorbisReader(strategy.Filename);
                strategy.Duration = mediaPlatformStream._reader.TotalTime;

                int samples = (mediaPlatformStream._reader.SampleRate * mediaPlatformStream._reader.Channels) / 2;
                mediaPlatformStream._sampleBuffer = new float[samples];
                mediaPlatformStream._dataBuffer = new byte[samples * sizeof(short)];

                mediaPlatformStream._player = new DynamicSoundEffectInstance(mediaPlatformStream._reader.SampleRate, (AudioChannels)mediaPlatformStream._reader.Channels);
                mediaPlatformStream._player.BufferNeeded += mediaPlatformStream.sfxi_BufferNeeded;
            }
        }

        internal void DestroyPlayer(MediaPlatformStream mediaPlatformStream)
        {
            if (mediaPlatformStream._player != null)
            {
                mediaPlatformStream._player.BufferNeeded -= mediaPlatformStream.sfxi_BufferNeeded;
                mediaPlatformStream._player.Dispose();
            }
            mediaPlatformStream._player = null;

            if (mediaPlatformStream._reader != null)
                mediaPlatformStream._reader.Dispose();
            mediaPlatformStream._reader = null;

            mediaPlatformStream._sampleBuffer = null;
            mediaPlatformStream._dataBuffer = null;
        }

        static internal unsafe int SubmitBuffer(MediaPlatformStream mediaPlatformStream, DynamicSoundEffectInstance sfxi, VorbisReader reader)
        {
            int count = mediaPlatformStream._reader.ReadSamples(mediaPlatformStream._sampleBuffer, 0, mediaPlatformStream._sampleBuffer.Length);
            if (count > 0)
            {
                fixed (float* pSampleBuffer = mediaPlatformStream._sampleBuffer)
                fixed (byte* pDataBuffer = mediaPlatformStream._dataBuffer)
                {
                    ConcreteMediaPlayerStrategy.ConvertFloat32ToInt16(pSampleBuffer, (short*)pDataBuffer, count);
                }
                sfxi.SubmitBuffer(mediaPlatformStream._dataBuffer, 0, count * sizeof(short));
            }

            return count;
        }

        static unsafe void ConvertFloat32ToInt16(float* fbuffer, short* outBuffer, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                int val = (int)(fbuffer[i] * short.MaxValue);
                val = Math.Min(val, +short.MaxValue);
                val = Math.Max(val, -short.MaxValue);
                outBuffer[i] = (short)val;
            }
        }

    }


}

