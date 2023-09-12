// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D
    {
        private void PlatformConstructRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
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
