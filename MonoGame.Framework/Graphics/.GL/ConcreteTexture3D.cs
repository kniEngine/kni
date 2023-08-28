// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ITexture3DStrategy, ITextureStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            this._width = width;
            this._height = height;
            this._depth = depth;
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


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }
        #endregion #region ITexture3DStrategy

    }
}
