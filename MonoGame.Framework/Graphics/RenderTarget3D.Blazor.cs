// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D : IRenderTargetGL
    {

        WebGLTexture IRenderTargetGL.GLTexture { get { return glTexture; } }
        WebGLTextureTarget IRenderTargetGL.GLTarget { get { return glTarget; } }
        WebGLTexture IRenderTargetGL.GLColorBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLDepthBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLStencilBuffer { get; set; }

        WebGLTextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            return glTarget;
        }

        private void PlatformConstructRenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);
            RenderTargetUsage = usage;

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
