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

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Reader != null)
                    return mediaPlatformStream.Reader.DecodedTime;
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

            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)queuedSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Volume = innerVolume;
            }
        }

        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Volume = innerVolume;

                mediaPlatformStream.CreatePlayer(((IPlatformSong)song).Strategy);

                DynamicSoundEffectInstance player = mediaPlatformStream.Player;

                SoundState state = player.State;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Paused:
                        player.Resume();
                        return;
                    case SoundState.Stopped:
                        player.Volume = innerVolume;
                        player.Play();
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
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Resume();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    mediaPlatformStream.Player.Stop();
                    mediaPlatformStream.DestroyPlayer();
                }
            }
        }

        protected internal override void PlatformClearQueue()
        {
            while (base.Queue.Count > 0)
            {
                Song song = base.Queue[0];

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    mediaPlatformStream.Player.Stop();
                    mediaPlatformStream.DestroyPlayer();
                }

                base.RemoveQueuedSong(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }

    }


    internal sealed class MediaPlatformStream : IDisposable
    {
        DynamicSoundEffectInstance _player; // TODO: Move _player to MediaPlayer
        VorbisReader _reader;
        float[] _sampleBuffer;
        byte[] _dataBuffer;

        internal DynamicSoundEffectInstance Player { get { return _player; } }
        internal VorbisReader Reader { get { return _reader; } }


        internal MediaPlatformStream(Uri streamSource)
        {
        }

        internal delegate void FinishedPlayingHandler();
        event FinishedPlayingHandler DonePlaying;

        /// <summary>
        /// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
        /// </summary>
        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            if (DonePlaying == null)
                DonePlaying += handler;
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
                _player.BufferNeeded += sfxi_BufferNeeded;
            }
        }

        private void sfxi_BufferNeeded(object sender, EventArgs e)
        {
            DynamicSoundEffectInstance sfxi = (DynamicSoundEffectInstance)sender;
            int count = SubmitBuffer(sfxi, _reader);

            if (count == 0 && sfxi.PendingBufferCount <= 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += Song_OnUpdate;
            }
        }

        private void Song_OnUpdate()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= Song_OnUpdate;

            FinishedPlayingHandler handler = DonePlaying;
            if (handler != null)
                handler();
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

        internal void DestroyPlayer()
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

        #region IDisposable
        ~MediaPlatformStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DestroyPlayer();

            }

            //base.Dispose(disposing);

        }
        #endregion
    }
}

