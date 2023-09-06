// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetGL
    {

        WebGLTexture IRenderTargetGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        WebGLTextureTarget IRenderTargetGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        WebGLTexture IRenderTargetGL.GLColorBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLDepthBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetGL.GLStencilBuffer { get; set; }

        WebGLTextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_X + arraySlice;
        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTargetCube)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

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
