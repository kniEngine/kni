// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
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

            return activeSong.Position;
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            if (Queue.ActiveSong == null)
                return;

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
                queuedSong.Volume = innerVolume;
            }
        }
        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong == null)
                return;

            song.SetEventHandler(OnSongFinishedPlaying);

            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

            song.Volume = innerVolume;
            song.Play();
        }

        protected override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong == null)
                return;

            activeSong.Pause();
        }

        protected override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong == null)
                return;

            activeSong.Resume();
        }

        protected override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                activeSong.Stop();
            }
        }

        protected override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];
                song.Stop();
                Queue.Remove(song);
            }

            //base.ClearQueue();
        }

    }
}

