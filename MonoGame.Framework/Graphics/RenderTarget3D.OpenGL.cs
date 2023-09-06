// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D
    {

        private void PlatformConstructRenderTarget3D(GraphicsDeviceStrategy deviceStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTarget3D)._multiSampleCount = deviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {                
            }

            base.Dispose(disposing);
        }

    }
}
