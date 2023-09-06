// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
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
