// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {

        private void PlatformConstructRenderTargetCube(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
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
