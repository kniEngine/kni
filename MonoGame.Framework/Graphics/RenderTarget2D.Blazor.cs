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

        private void PlatformConstructRenderTarget2D(GraphicsDeviceStrategy deviceStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._multiSampleCount = deviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            //Threading.EnsureUIThread();
            {
                ConcreteTexture.PlatformCreateRenderTarget(this, deviceStrategy, width, height, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
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

            ConcreteTexture.PlatformDeleteRenderTarget(this, GraphicsDevice.Strategy);

            base.Dispose(disposing);
        }
    }
}
