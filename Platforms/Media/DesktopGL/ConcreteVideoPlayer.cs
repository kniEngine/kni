// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;


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
            set { base.IsLooped = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new NotImplementedException();
            }
        }

        public override TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            throw new NotImplementedException();
        }

        public override void PlatformPlay(Video video)
        {
            throw new NotImplementedException();
        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformResume()
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException();
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
