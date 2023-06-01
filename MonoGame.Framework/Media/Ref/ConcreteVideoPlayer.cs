// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set
            {
                base.IsLooped = value;
                throw new PlatformNotSupportedException();
            }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new PlatformNotSupportedException();
            }
        }

        public override TimeSpan PlayPosition
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    PlatformSetVolume();
            }
        }

        internal ConcreteVideoPlayerStrategy()
        {
            throw new PlatformNotSupportedException();
        }

        public override Texture2D PlatformGetTexture()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformPlay(Video video)
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


        private void PlatformSetVolume()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
