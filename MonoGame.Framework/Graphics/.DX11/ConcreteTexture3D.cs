// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
        }

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
        }
        #endregion #region ITexture3DStrategy


        internal bool _mipMap;
        internal bool _isRenderTarget;

    }
}
