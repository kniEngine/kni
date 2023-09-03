// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture2D : ConcreteTexture, ITexture2DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _arraySize;

        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;
        }


        #region ITexture2DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int ArraySize { get { return _arraySize; } }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, this._width, this._height); }
        }
        #endregion #region ITexture2DStrategy


        internal bool _mipMap;
        internal bool _shared;
    }
}
