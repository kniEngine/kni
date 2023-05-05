// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using AudioToolbox;
using AVFoundation;
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
            if (activeSong != null)
                return ((ConcreteSongStrategy)activeSong.Strategy).Position;

            return TimeSpan.Zero;
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
            return !AVAudioSession.SharedInstance().OtherAudioPlaying;
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            
            foreach (Song queuedSong in Queue.Songs)
            {
                ((ConcreteSongStrategy)queuedSong.Strategy).Volume = innerVolume;
            }
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong == null)
                return;

            ((ConcreteSongStrategy)song.Strategy).SetEventHandler(OnSongFinishedPlaying);

            float innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();

            ((ConcreteSongStrategy)song.Strategy).Volume = innerVolume;
            ((ConcreteSongStrategy)song.Strategy).Play();
        }

        protected override void PlatformPause()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong == null)
                return;

            ((ConcreteSongStrategy)activeSong.Strategy).Pause();
        }

        protected override void PlatformResume()
        {
            Song activeSong = Queue.ActiveSong;
            if (activeSong == null)
                return;

            ((ConcreteSongStrategy)activeSong.Strategy).Resume();
        }

        protected override void PlatformStop()
        {
            foreach (Song queuedSong in Queue.Songs)
            {
                var activeSong = Queue.ActiveSong;
                ((ConcreteSongStrategy)activeSong.Strategy).Stop();
            }
        }

        protected override void PlatformClearQueue()
        {
            while (Queue.Count > 0)
            {
                Song song = Queue[0];
                ((ConcreteSongStrategy)song.Strategy).Stop();
                Queue.Remove(song);
            }

            //base.ClearQueue();
        }

    }
}

