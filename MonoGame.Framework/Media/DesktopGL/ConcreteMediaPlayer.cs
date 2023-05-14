// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


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

            if (((ConcreteSongStrategy)activeSong.Strategy).Reader != null)
                return ((ConcreteSongStrategy)activeSong.Strategy).Reader.DecodedTime;
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
                if (((ConcreteSongStrategy)queuedSong.Strategy).Player != null)
                    ((ConcreteSongStrategy)queuedSong.Strategy).Player.Volume = innerVolume;
            }
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                ((ConcreteSongStrategy)song.Strategy).SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

                if (((ConcreteSongStrategy)song.Strategy).Player != null)
                    ((ConcreteSongStrategy)song.Strategy).Player.Volume = innerVolume;

                ((ConcreteSongStrategy)song.Strategy).CreatePlayer();

                var player = ((ConcreteSongStrategy)song.Strategy).Player;

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

        protected override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                    ((ConcreteSongStrategy)activeSong.Strategy).Player.Pause();
            }
        }

        protected override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                    ((ConcreteSongStrategy)activeSong.Strategy).Player.Resume();
            }
        }

        protected override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                {
                    ((ConcreteSongStrategy)activeSong.Strategy).Player.Stop();
                    ((ConcreteSongStrategy)activeSong.Strategy).DestroyPlayer();
                }
            }
        }

        protected override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];
                if (((ConcreteSongStrategy)song.Strategy).Player != null)
                {
                    ((ConcreteSongStrategy)song.Strategy).Player.Stop();
                    ((ConcreteSongStrategy)song.Strategy).DestroyPlayer();
                }

                Queue.Remove(song);
            }

            //base.ClearQueue();
        }

    }
}

