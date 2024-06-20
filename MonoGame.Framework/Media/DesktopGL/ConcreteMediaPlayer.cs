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
                if (mediaPlatformStream._reader != null)
                    return mediaPlatformStream._reader.DecodedTime;
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
                if (mediaPlatformStream._player != null)
                    mediaPlatformStream._player.Volume = innerVolume;
            }
        }

        public override void PlatformPlaySong(Song song)
        {
            if (base.Queue.ActiveSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                mediaPlatformStream.SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;

                if (mediaPlatformStream._player != null)
                    mediaPlatformStream._player.Volume = innerVolume;

                ConcreteMediaPlayerStrategy.CreatePlayer(mediaPlatformStream, ((IPlatformSong)song).Strategy);

                DynamicSoundEffectInstance player = mediaPlatformStream._player;

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
                if (mediaPlatformStream._player != null)
                    mediaPlatformStream._player.Pause();
            }
        }

        public override void PlatformResume()
        {
            Song activeSong = base.Queue.ActiveSong;
            if (activeSong != null)
            {
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream._player != null)
                    mediaPlatformStream._player.Resume();
            }
        }

        public override void PlatformStop()
        {
            for (int i = 0; i < base.Queue.Count; i++)
            {
                Song queuedSong = base.Queue[i];

                Song activeSong = base.Queue.ActiveSong;
                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)activeSong).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream._player != null)
                {
                    mediaPlatformStream._player.Stop();
                    MediaPlatformStream.DestroyPlayer(mediaPlatformStream);
                }
            }
        }

        protected override void PlatformClearQueue()
        {
            while (base.Queue.Count > 0)
            {
                Song song = base.Queue[0];

                MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
                if (mediaPlatformStream._player != null)
                {
                    mediaPlatformStream._player.Stop();
                    MediaPlatformStream.DestroyPlayer(mediaPlatformStream);
                }

                base.RemoveQueuedSong(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }


        internal delegate void FinishedPlayingHandler();

        internal static void CreatePlayer(MediaPlatformStream mediaPlatformStream, SongStrategy strategy)
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

