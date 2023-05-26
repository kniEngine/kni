// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

        internal override bool PlatformGetIsMuted()
        {
            return base.PlatformGetIsMuted();
        }

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            if (Queue.Count == 0)
                return;

            SetChannelVolumes();
        }

        internal override bool PlatformGetIsRepeating()
        {
            return base.PlatformGetIsRepeating();
        }

        internal override void PlatformSetIsRepeating(bool repeating)
        {
            base.PlatformSetIsRepeating(repeating);
        }

        internal override bool PlatformGetIsShuffled()
        {
            return base.PlatformGetIsShuffled();
        }

        internal override void PlatformSetIsShuffled(bool shuffled)
        {
            base.PlatformSetIsShuffled(shuffled);
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override float PlatformGetVolume()
        {
            return base.PlatformGetVolume();
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
                ((ConcreteSongStrategy)queuedSong.Strategy).Volume = innerVolume;
            }
        }

        internal override void PlatformPlaySong(Song song)
        {
            throw new NotImplementedException();
        }

        internal override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformResume()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformStop()
        {
            throw new NotImplementedException();
        }

    }
}

