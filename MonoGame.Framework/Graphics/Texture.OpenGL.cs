// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {

        private void PlatformGraphicsDeviceResetting()
        {
            if (GetTextureStrategy<ConcreteTexture>()._glTexture > 0)
            {
                if (!GraphicsDevice.IsDisposed)
                {
                    GL.DeleteTexture(GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            GetTextureStrategy<ConcreteTexture>()._glTexture = -1;

            GetTextureStrategy<ConcreteTexture>()._glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GetTextureStrategy<ConcreteTexture>()._glTexture > 0)
                {
                    if (!GraphicsDevice.IsDisposed)
                    {
                        GL.DeleteTexture(GetTextureStrategy<ConcreteTexture>()._glTexture);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                GetTextureStrategy<ConcreteTexture>()._glTexture = -1;

                GetTextureStrategy<ConcreteTexture>()._glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

    }
}

