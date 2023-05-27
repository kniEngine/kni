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

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            if (Queue.Count == 0)
                return;

            SetChannelVolumes();
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong == null)
                return TimeSpan.Zero;

            MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
            if (mediaPlatformStream.Reader != null)
                return mediaPlatformStream.Reader.DecodedTime;
            else
                return TimeSpan.Zero;
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            if (Queue.ActiveSong != null)
                SetChannelVolumes();
        }

        internal override bool PlatformGetGameHasControl()
        {
            return true;
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            
            foreach (Song queuedSong in Queue.Songs)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)queuedSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Volume = innerVolume;
            }
        }

        internal override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)song.Strategy).GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Volume = innerVolume;

                mediaPlatformStream.CreatePlayer(song.Strategy);

                var player = mediaPlatformStream.Player;

                var state = player.State;
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
                        song.Strategy.PlayCount++;
                        return;
                }
            }

        }

        internal override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Pause();
            }
        }

        internal override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                    mediaPlatformStream.Player.Resume();
            }
        }

        internal override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)activeSong.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    mediaPlatformStream.Player.Stop();
                    mediaPlatformStream.DestroyPlayer();
                }
            }
        }

        internal override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];

                MediaPlatformStream mediaPlatformStream = ((ConcreteSongStrategy)song.Strategy).GetMediaPlatformStream();
                if (mediaPlatformStream.Player != null)
                {
                    mediaPlatformStream.Player.Stop();
                    mediaPlatformStream.DestroyPlayer();
                }

                Queue.Remove(song);
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

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
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

