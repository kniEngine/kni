// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;


        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format,
                                   bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this.PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            throw new NotImplementedException();
        }

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
            throw new NotImplementedException();
        }
        #endregion ITexture3DStrategy

        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this, ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy);

            base.Dispose(disposing);
        }

    }
}
