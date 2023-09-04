// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetGL
    {
        int IRenderTargetGL.GLTexture { get { return GetTextureStrategy<ConcreteTexture>()._glTexture; } }
        TextureTarget IRenderTargetGL.GLTarget { get { return GetTextureStrategy<ConcreteTexture>()._glTarget; } }
        int IRenderTargetGL.GLColorBuffer { get; set; }
        int IRenderTargetGL.GLDepthBuffer { get; set; }
        int IRenderTargetGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            return TextureTarget.TextureCubeMapPositiveX + arraySlice;
        }

        private void PlatformConstructRenderTargetCube(GraphicsDeviceStrategy deviceStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = deviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            Threading.EnsureUIThread();
            {
                ConcreteTexture.PlatformCreateRenderTarget(this, deviceStrategy, this.Size, this.Size, mipMap, this.Format, preferredDepthFormat, MultiSampleCount);
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
