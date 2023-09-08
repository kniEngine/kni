// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetStrategyGL
    {
        int IRenderTargetStrategyGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        TextureTarget IRenderTargetStrategyGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        int IRenderTargetStrategyGL.GLColorBuffer { get; set; }
        int IRenderTargetStrategyGL.GLDepthBuffer { get; set; }
        int IRenderTargetStrategyGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            return TextureTarget.TextureCubeMapPositiveX + arraySlice;
        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            Threading.EnsureUIThread();
            {
                ConcreteTexture.PlatformCreateRenderTarget(this, contextStrategy.Context.DeviceStrategy, this.Size, this.Size, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
            }
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
