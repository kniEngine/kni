// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoStrategy : VideoStrategy
    {
        internal VideoPlatformStream _videoPlatformStream;

        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {

            this._videoPlatformStream = new VideoPlatformStream(this.FileName);
        }

        internal VideoPlatformStream GetVideoPlatformStream()
        {
            return _videoPlatformStream;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_videoPlatformStream != null)
                {
                    _videoPlatformStream.Dispose();
                    _videoPlatformStream = null;
                }

            }

            base.Dispose(disposing);
        }
    }

}
