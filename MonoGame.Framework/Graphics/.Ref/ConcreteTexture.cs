// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture : GraphicsResourceStrategy, ITextureStrategy
    {

        internal ConcreteTexture(GraphicsContextStrategy contextStrategy, SurfaceFormat format, int levelCount)
            : base(contextStrategy)
        {
        
        }


        #region ITextureStrategy
        public SurfaceFormat Format { get { throw new PlatformNotSupportedException(); } }
        public int LevelCount { get { throw new PlatformNotSupportedException(); } }
        #endregion ITextureStrategy


        internal override void PlatformGraphicsContextLost()
        {
            throw new PlatformNotSupportedException();

            base.PlatformGraphicsContextLost();
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
