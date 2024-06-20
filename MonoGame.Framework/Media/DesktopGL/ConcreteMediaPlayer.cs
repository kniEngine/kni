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

                mediaPlatformStream.CreatePlayer(((IPlatformSong)song).Strategy);

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
                    mediaPlatformStream.DestroyPlayer();
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
                    mediaPlatformStream.DestroyPlayer();
                }

                base.RemoveQueuedSong(song);
            }

            _numSongsInQueuePlayed = 0;
            //base.ClearQueue();
        }

    }


}

