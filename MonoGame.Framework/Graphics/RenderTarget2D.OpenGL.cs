// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D : IRenderTargetGL
    {
        int IRenderTargetGL.GLTexture { get { return _glTexture; } }
        TextureTarget IRenderTargetGL.GLTarget { get { return _glTarget; } }
        int IRenderTargetGL.GLColorBuffer { get; set; }
        int IRenderTargetGL.GLDepthBuffer { get; set; }
        int IRenderTargetGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            if (arraySlice != 0)
                throw new NotImplementedException("arraySlice is not implemented for Texture2D");

            return _glTarget;
        }

        private void PlatformConstructRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(preferredMultiSampleCount);
            RenderTargetUsage = usage;

            Threading.EnsureUIThread();
            {
                base.PlatformCreateRenderTarget(
                    graphicsDevice, width, height, mipMap, this.Format, preferredDepthFormat, MultiSampleCount, usage);
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
                    base.PlatformDeleteRenderTarget();
                }
            }

            base.Dispose(disposing);
        }
    }
}
