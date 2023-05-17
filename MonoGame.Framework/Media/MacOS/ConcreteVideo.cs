// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoStrategy : VideoStrategy
    {
        internal VideoPlatformStream _videoPlatformStream;
        
        internal TimeSpan CurrentPosition
        {
            get { return new TimeSpan(_movie.CurrentTime.Value); }
        }

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
