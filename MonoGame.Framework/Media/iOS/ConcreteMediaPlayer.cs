// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;
using AudioToolbox;
using AVFoundation;
using CoreMedia;


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
            if (activeSong != null)
            {
                AVPlayer player = ((ConcreteSongStrategy)activeSong.Strategy).Player;
                return TimeSpan.FromSeconds(player.CurrentTime.Seconds);
            }

            return TimeSpan.Zero;
        }

        internal void PlatformSetPlayPosition(TimeSpan value)
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                AVPlayer player = ((ConcreteSongStrategy)activeSong.Strategy).Player;
                player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1000));
            }
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
            return !AVAudioSession.SharedInstance().OtherAudioPlaying;
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            
            foreach (Song queuedSong in Queue.Songs)
            {
                if (((ConcreteSongStrategy)queuedSong.Strategy).Player != null)
                {
                    AVPlayer player = ((ConcreteSongStrategy)queuedSong.Strategy).Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }
            }
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong != null)
            {
                ((ConcreteSongStrategy)song.Strategy).SetEventHandler(OnSongFinishedPlaying);

                float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

                if (((ConcreteSongStrategy)song.Strategy).Player != null)
                {
                    AVPlayer player = ((ConcreteSongStrategy)song.Strategy).Player;
                    if (player.Volume != innerVolume)
                        player.Volume = innerVolume;
                }

                if (((ConcreteSongStrategy)song.Strategy).Player == null)
                {
                    // MediaLibrary items are lazy loaded
                    if (((ConcreteSongStrategy)song.Strategy).AssetUrl != null)
                        ((ConcreteSongStrategy)song.Strategy).CreatePlayer(((ConcreteSongStrategy)song.Strategy).AssetUrl);
                    else
                        return;
                }

                AVPlayer player2 = ((ConcreteSongStrategy)song.Strategy).Player;
                player2.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
                player2.Play();

                song.Strategy.PlayCount++;
            }
        }

        protected override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                {
                    AVPlayer player = ((ConcreteSongStrategy)activeSong.Strategy).Player;
                    player.Pause();
                }
            }
        }

        protected override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong != null)
            {
                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                {
                    AVPlayer player = ((ConcreteSongStrategy)activeSong.Strategy).Player;
                    player.Play();
                }
            }
        }

        protected override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;

                if (((ConcreteSongStrategy)activeSong.Strategy).Player != null)
                {
                    AVPlayer player = ((ConcreteSongStrategy)activeSong.Strategy).Player;
                    player.Pause();
                    activeSong.Strategy.PlayCount = 0;
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
                    AVPlayer player = ((ConcreteSongStrategy)song.Strategy).Player;
                    player.Pause();
                    song.Strategy.PlayCount = 0;
                }

                Queue.Remove(song);
            }

            //base.ClearQueue();
        }

    }
}

