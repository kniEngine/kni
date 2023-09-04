// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D : IRenderTargetGL
    {

        WebGLTexture IRenderTargetGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        WebGLTextureTarget IRenderTargetGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        WebGLTexture IRenderTargetGL.GLColorBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLDepthBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLStencilBuffer { get; set; }

        WebGLTextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            if (arraySlice != 0)
                throw new NotImplementedException("arraySlice is not implemented for Texture2D");

            return GetTextureStrategy<ConcreteTexture>()._glTarget;
        }

        private void PlatformConstructRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            //Threading.EnsureUIThread();
            {
                base.PlatformCreateRenderTarget(
                    graphicsDevice, width, height, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.PlatformDeleteRenderTarget();

            base.Dispose(disposing);
        }
    }
}
