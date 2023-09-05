// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture3D : Texture
    {

        private void PlatformConstructTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = TextureTarget.Texture3D;

            Threading.EnsureUIThread();
            {
                GetTextureStrategy<ConcreteTexture>()._glTexture = GL.GenTexture();
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(GetTextureStrategy<ConcreteTexture>()._glTarget, GetTextureStrategy<ConcreteTexture>()._glTexture);
                GraphicsExtensions.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, GraphicsDevice, out GetTextureStrategy<ConcreteTexture>()._glInternalFormat, out GetTextureStrategy<ConcreteTexture>()._glFormat, out GetTextureStrategy<ConcreteTexture>()._glType);

                GL.TexImage3D(GetTextureStrategy<ConcreteTexture>()._glTarget, 0, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, width, height, depth, 0, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, IntPtr.Zero);
                GraphicsExtensions.CheckGLError();
            }

            if (mipMap)
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
        }

    }
}

