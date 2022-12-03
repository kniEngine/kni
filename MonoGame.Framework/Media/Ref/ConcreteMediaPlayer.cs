// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {

        internal ConcreteMediaPlayerStrategy()
        {
            throw new PlatformNotSupportedException();
        }

        #region Properties

        internal override bool PlatformGetIsMuted()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetIsMuted(bool muted)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformGetIsRepeating()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetIsRepeating(bool repeating)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformGetIsShuffled()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetIsShuffled(bool shuffled)
        {
            throw new PlatformNotSupportedException();
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            throw new PlatformNotSupportedException();
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            throw new PlatformNotSupportedException();
        }

        internal override float PlatformGetVolume()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetVolume(float volume)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformGetGameHasControl()
        {
            throw new PlatformNotSupportedException();
        }

        #endregion

        protected override void PlatformPlaySong(Song song)
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformPause()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformResume()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

    }
}

