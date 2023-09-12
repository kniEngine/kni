// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetCube : ConcreteTextureCube, IRenderTargetCubeStrategy, IRenderTargetStrategy
    {
        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
            : base(contextStrategy, size, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {

            PlatformConstructTextureCube_rt(contextStrategy, size, mipMap, preferredSurfaceFormat);
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


        private void PlatformConstructTextureCube_rt(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            base.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }

    }
}
