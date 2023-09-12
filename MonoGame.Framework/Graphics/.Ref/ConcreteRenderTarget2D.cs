// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : ConcreteTexture2D, IRenderTarget2DStrategy, IRenderTargetStrategy
    {
        internal int _multiSampleCount;

        internal ConcreteRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
              Texture2D.SurfaceType surfaceType)
            : base(contextStrategy, width, height, mipMap, preferredSurfaceFormat, arraySize, shared,
                   isRenderTarget: true)
        {

            if (surfaceType == Texture2D.SurfaceType.RenderTargetSwapChain)
            {
                // Texture will be created by the RenderTargetSwapChain.
                return;
            }

            PlatformConstructTexture2D_rt(contextStrategy, width, height, mipMap, preferredSurfaceFormat, shared);
        }


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
        #endregion IRenderTarget2DStrategy


        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            base.PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }
    }
}
