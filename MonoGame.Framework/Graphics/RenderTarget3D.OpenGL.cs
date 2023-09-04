// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D
    {

        private void PlatformConstructRenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

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
