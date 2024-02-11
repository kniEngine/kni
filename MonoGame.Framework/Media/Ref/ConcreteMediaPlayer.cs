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

        public override bool PlatformIsMuted
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        internal override bool PlatformIsRepeating
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        internal override bool PlatformIsShuffled
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get { throw new PlatformNotSupportedException(); }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            throw new PlatformNotSupportedException();
        }

        public override float PlatformVolume
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override bool PlatformGameHasControl
        {
            get { throw new PlatformNotSupportedException(); }
        }

        #endregion

        public override void PlatformPlaySong(Song song)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformPause()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformResume()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

    }
}

