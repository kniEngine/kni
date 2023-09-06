// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : IRenderTarget2DStrategy, ITexture2DStrategy
    {
        internal int _multiSampleCount;

        internal ConcreteRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, RenderTargetUsage usage,
            DepthFormat preferredDepthFormat)
        {

        }


        #region ITextureStrategy
        public SurfaceFormat Format
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int LevelCount
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion #region ITextureStrategy


        #region ITexture2DStrategy
        public int Width
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int Height
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int ArraySize
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public Rectangle Bounds
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public IntPtr GetSharedHandle()
        {
            throw new PlatformNotSupportedException();
        }

        public void SetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new PlatformNotSupportedException();
        }
        #endregion #region ITexture2DStrategy


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int MultiSampleCount
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion #region IRenderTarget2DStrategy

    }
}
