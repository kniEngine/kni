// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetStrategyGL
    {

        WebGLTexture IRenderTargetStrategyGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        WebGLTextureTarget IRenderTargetStrategyGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        WebGLTexture IRenderTargetStrategyGL.GLColorBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLDepthBuffer { get; set; }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLStencilBuffer { get; set; }

        WebGLTextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_X + arraySlice;
        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

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
