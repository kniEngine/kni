// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {
            this._size = size;
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
        }
        #endregion #region ITextureCubeStrategy


        internal bool _mipMap;
        internal bool _isRenderTarget;
    }
}
