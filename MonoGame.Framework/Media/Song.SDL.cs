// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Media;
using NVorbis;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        DynamicSoundEffectInstance _player; // TODO: Move _player to MediaPlayer
        VorbisReader _reader;
        float[] _sampleBuffer;
        byte[] _dataBuffer;

        private float _volume = 1f;


        internal override Album PlatformGetAlbum()
        {
            return null;
        }

        internal override void PlatformSetAlbum(Album album)
        {

        }

        internal override Artist PlatformGetArtist()
        {
            return null;
        }

        internal override Genre PlatformGetGenre()
        {
            return null;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            return false;
        }

        internal override bool PlatformIsRated()
        {
            return false;
        }

        internal override string PlatformGetName()
        {
            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }


        internal override void PlatformInitialize(string fileName)
        {
        }

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
        event FinishedPlayingHandler DonePlaying;

        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            if (DonePlaying == null)
                DonePlaying += handler;
        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                DestroyPlayer();
            }
        }

        internal void Play()
        {
            CreatePlayer();

            var state = _player.State;
            switch (_player.State)
            {
                case SoundState.Playing:
                    return;
                case SoundState.Paused:
                    _player.Resume();
                    return;
                case SoundState.Stopped:
                    _player.Volume = _volume;
                    _player.Play();
                    _playCount++;
                    return;
            }
        }

        internal void Pause()
        {
            if (_player != null)
                _player.Pause();
        }

        internal void Resume()
        {
            if (_player != null)
                _player.Resume();
        }

        internal void Stop()
        {
            if (_player != null)
            {
                _player.Stop();
                DestroyPlayer();
            }
        }

        internal float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_player != null)
                    _player.Volume = _volume;
            }
        }

        internal TimeSpan Position
        {
            get
            {
                if (_reader != null)
                    return _reader.DecodedTime;

                return TimeSpan.Zero;
            }
        }

        private void CreatePlayer()
        {
            if (_player == null)
            {
                _reader = new VorbisReader(_name);
                _duration = _reader.TotalTime;

                int samples = (_reader.SampleRate * _reader.Channels) / 2;
                _sampleBuffer = new float[samples];
                _dataBuffer = new byte[samples * sizeof(short)];

                _player = new DynamicSoundEffectInstance(_reader.SampleRate, (AudioChannels)_reader.Channels);
                _player.BufferNeeded += sfxi_BufferNeeded;
            }
        }

        private void DestroyPlayer()
        {
            if (_player != null)
            {
                _player.BufferNeeded -= sfxi_BufferNeeded;
                _player.Dispose();
            }
            _player = null;

            if (_reader != null)
                _reader.Dispose();
            _reader = null;

            _sampleBuffer = null;
            _dataBuffer = null;
        }

        private void sfxi_BufferNeeded(object sender, EventArgs e)
        {
            var sfxi = (DynamicSoundEffectInstance)sender;
            int count = SubmitBuffer(sfxi, _reader);

            if (count == 0 && sfxi.PendingBufferCount <= 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += Song_OnUpdate;
            }
        }

        private void Song_OnUpdate()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= Song_OnUpdate;

            var handler = DonePlaying;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private unsafe int SubmitBuffer(DynamicSoundEffectInstance sfxi, VorbisReader reader)
        {
            int count = _reader.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);
            if (count > 0)
            {
                fixed (float* pSampleBuffer = _sampleBuffer)
                fixed (byte* pDataBuffer = _dataBuffer)
                {
                    ConvertFloat32ToInt16(pSampleBuffer, (short*)pDataBuffer, count);
                }
                sfxi.SubmitBuffer(_dataBuffer, 0, count * sizeof(short));
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

