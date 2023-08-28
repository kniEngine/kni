// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ITextureCubeStrategy, ITextureStrategy
    {
        private readonly int _size;

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            this._size = size;
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


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }
        #endregion #region ITextureCubeStrategy

    }
}
