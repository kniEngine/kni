// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
            if (Queue.ActiveSong == null)
                return TimeSpan.Zero;

            return Queue.ActiveSong.Position;
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

        private void SetChannelVolumes()
        {
            var innerVolume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            foreach (var song in Queue.Songs)
                song.Volume = innerVolume;
        }

        internal override bool PlatformGetGameHasControl()
        {
            return !AVAudioSession.SharedInstance().OtherAudioPlaying;
        }

        #endregion

        protected override void PlatformPause()
        {
            if (Queue.ActiveSong == null)
                return;

            Queue.ActiveSong.Pause();
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (Queue.ActiveSong == null)
                return;

            song.SetEventHandler(OnSongFinishedPlaying);

            song.Volume = base.PlatformGetIsMuted() ? 0.0f : base.PlatformGetVolume();
            song.Play();
        }

        protected override void PlatformResume()
        {
            if (Queue.ActiveSong == null)
                return;

            Queue.ActiveSong.Resume();
        }

        protected override void PlatformStop()
        {            
            foreach (var song in Queue.Songs)
                Queue.ActiveSong.Stop();
        }




    }
}

