// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            ((ConcreteTexture2D)_strategyTexture2D).PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            Threading.EnsureUIThread();
            {
                ConcreteTexture.PlatformCreateRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTarget2D, contextStrategy.Context.DeviceStrategy, width, height, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this._strategyRenderTarget2D, GraphicsDevice.Strategy);
                }
            }

            base.Dispose(disposing);
        }
    }
}
