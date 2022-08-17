// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
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
