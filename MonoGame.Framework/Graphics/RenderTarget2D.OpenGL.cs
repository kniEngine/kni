// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D : IRenderTargetStrategyGL
    {
        int IRenderTargetStrategyGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        TextureTarget IRenderTargetStrategyGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        int IRenderTargetStrategyGL.GLColorBuffer { get; set; }
        int IRenderTargetStrategyGL.GLDepthBuffer { get; set; }
        int IRenderTargetStrategyGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            if (arraySlice != 0)
                throw new NotImplementedException("arraySlice is not implemented for Texture2D");

            return GetTextureStrategy<ConcreteTexture>()._glTarget;
        }

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            Threading.EnsureUIThread();
            {
                ConcreteTexture.PlatformCreateRenderTarget(this, contextStrategy.Context.DeviceStrategy, width, height, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
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
                    ConcreteTexture.PlatformDeleteRenderTarget(this, GraphicsDevice.Strategy);
                }
            }

            base.Dispose(disposing);
        }
    }
}
