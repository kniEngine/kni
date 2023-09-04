// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture3D : Texture
    {

        private void PlatformConstructTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
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

       
        private void PlatformSetData<T>(
            int level,
            int left, int top, int right, int bottom, int front, int back,
            T[] data, int startIndex, int elementCount)
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            Threading.EnsureUIThread();

            {
                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    GL.BindTexture(GetTextureStrategy<ConcreteTexture>()._glTarget, GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GraphicsExtensions.CheckGLError();

                    GL.TexSubImage3D(GetTextureStrategy<ConcreteTexture>()._glTarget, level, left, top, front, width, height, depth, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, dataPtr);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }
            }

        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
             where T : struct
        {

            throw new NotImplementedException();
        }
    }
}

