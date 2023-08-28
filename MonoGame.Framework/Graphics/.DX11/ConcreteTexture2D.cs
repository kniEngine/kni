// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture2D : ITexture2DStrategy, ITextureStrategy
    {
        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipmap)
        {

        }


        #region ITextureStrategy
        public SurfaceFormat Format
        {
            get { throw new NotImplementedException(); }
        }

        public int LevelCount
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region ITextureStrategy


        #region ITexture2DStrategy
        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public Rectangle Bounds
        {
            get { throw new NotImplementedException(); }
        }
        #endregion #region ITexture2DStrategy

    }
}
