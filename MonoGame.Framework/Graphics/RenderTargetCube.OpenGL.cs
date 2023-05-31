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
        int IRenderTargetGL.GLTexture { get { return _glTexture; } }
        TextureTarget IRenderTargetGL.GLTarget { get { return _glTarget; } }
        int IRenderTargetGL.GLColorBuffer { get; set; }
        int IRenderTargetGL.GLDepthBuffer { get; set; }
        int IRenderTargetGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            return TextureTarget.TextureCubeMapPositiveX + arraySlice;
        }

        private void PlatformConstructRenderTargetCube(
            GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.GetClampedMultisampleCount(preferredMultiSampleCount);
            RenderTargetUsage = usage;

            Threading.EnsureUIThread();
            {
                graphicsDevice.PlatformCreateRenderTarget(
                    this, _size, _size, mipMap, this.Format, preferredDepthFormat, MultiSampleCount, usage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    GraphicsDevice.PlatformDeleteRenderTarget(this);
                }
            }

            base.Dispose(disposing);
        }
    }
}
