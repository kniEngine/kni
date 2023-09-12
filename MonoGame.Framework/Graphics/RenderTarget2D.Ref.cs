// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
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
