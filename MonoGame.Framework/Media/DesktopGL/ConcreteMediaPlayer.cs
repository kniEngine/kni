// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using NVorbis;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        private DynamicSoundEffectInstance _player;
        private VorbisReader _reader;
        private float[] _sampleBuffer;
        private byte[] _dataBuffer;

        internal ConcreteMediaPlayerStrategy()
        {
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

                if (_reader != null)
                    return _reader.DecodedTime;
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

            if (_player != null)
                _player.Volume = innerVolume;
        }

        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                if (_player != null)
                    _player.Volume = innerVolume;

                this.CreatePlayer(((IPlatformSong)song).Strategy);

                SoundState state = _player.State;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Paused:
                        _player.Resume();
                        return;
                    case SoundState.Stopped:
                        _player.Volume = innerVolume;
                        _player.Play();
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
                if (_player != null)
                    _player.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                if (_player != null)
                    _player.Resume();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;

                if (_player != null)
                {
                    _player.Stop();
                    this.DestroyPlayer();
                }
            }
        }

        protected override void PlatformClearQueue()
        {
            while (base.Queue.Count > 0)
            {
                Song song = base.Queue[0];

                if (_player != null)
                {
                    _player.Stop();
                    this.DestroyPlayer();
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
                if (_player != null)
                {
                    _player.BufferNeeded -= this.sfxi_BufferNeeded;
                    _player.Dispose();
                }
                _player = null;

                if (_reader != null)
                    _reader.Dispose();
                _reader = null;

                _sampleBuffer = null;
                _dataBuffer = null;

            }

            base.Dispose(disposing);
        }

        #endregion


        internal unsafe void sfxi_BufferNeeded(object sender, EventArgs e)
        {
            DynamicSoundEffectInstance sfxi = (DynamicSoundEffectInstance)sender;

            // Submit Buffer
            int count = _reader.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);
            if (count > 0)
            {
                fixed (float* pSampleBuffer = _sampleBuffer)
                fixed (byte*  pDataBuffer = _dataBuffer)
                {
                    ConcreteMediaPlayerStrategy.ConvertFloat32ToInt16(pSampleBuffer, (short*)pDataBuffer, count);
                }
                sfxi.SubmitBuffer(_dataBuffer, 0, count * sizeof(short));
            }


            if (count == 0 && sfxi.PendingBufferCount <= 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += Song_OnUpdate;
            }
        }

        private void Song_OnUpdate()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= Song_OnUpdate;

            this.OnSongFinishedPlaying();
        }

        internal void CreatePlayer(SongStrategy strategy)
        {
            if (_player == null)
            {
                _reader = new VorbisReader(strategy.Filename);
                strategy.Duration = _reader.TotalTime;

                int samples = (_reader.SampleRate * _reader.Channels) / 2;
                _sampleBuffer = new float[samples];
                _dataBuffer = new byte[samples * sizeof(short)];

                _player = new DynamicSoundEffectInstance(_reader.SampleRate, (AudioChannels)_reader.Channels);
                _player.BufferNeeded += this.sfxi_BufferNeeded;
            }
        }

        internal void DestroyPlayer()
        {
            if (_player != null)
            {
                _player.BufferNeeded -= this.sfxi_BufferNeeded;
                _player.Dispose();
            }
            _player = null;

            if (_reader != null)
                _reader.Dispose();
            _reader = null;

            _sampleBuffer = null;
            _dataBuffer = null;
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

