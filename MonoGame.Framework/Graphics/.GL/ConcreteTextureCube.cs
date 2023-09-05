// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {
            this._size = size;
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {

            Threading.EnsureUIThread();

            {
                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                // Use try..finally to make sure dataHandle is freed in case of an error
                try
                {
                    int startBytes = startIndex * elementSizeInByte;
                    IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
                    GraphicsExtensions.CheckGLError();

                    TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
                    if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                            _glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureUIThread();

#if OPENGL && DESKTOPGL
            TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);

            if (_glFormat == GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                int pixelToT = Format.GetSize() / tSizeInByte;
                int tFullWidth = Math.Max(this.Size >> level, 1) / 4 * pixelToT;
                T[] temp = new T[Math.Max(this.Size >> level, 1) / 4 * tFullWidth];
                GL.GetCompressedTexImage(target, level, temp);
                GraphicsExtensions.CheckGLError();

                int rowCount = checkedRect.Height / 4;
                int tRectWidth = checkedRect.Width / 4 * Format.GetSize() / tSizeInByte;
                for (int r = 0; r < rowCount; r++)
                {
                    int tempStart = checkedRect.X / 4 * pixelToT + (checkedRect.Top / 4 + r) * tFullWidth;
                    int dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
            else
            {
                // we need to convert from our format size to the size of T here
                int tFullWidth = Math.Max(this.Size >> level, 1) * Format.GetSize() / tSizeInByte;
                T[] temp = new T[Math.Max(this.Size >> level, 1) * tFullWidth];
                GL.GetTexImage(target, level, _glFormat, _glType, temp);
                GraphicsExtensions.CheckGLError();

                int pixelToT = Format.GetSize() / tSizeInByte;
                int rowCount = checkedRect.Height;
                int tRectWidth = checkedRect.Width * pixelToT;
                for (int r = 0; r < rowCount; r++)
                {
                    int tempStart = checkedRect.X * pixelToT + (r + checkedRect.Top) * tFullWidth;
                    int dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
#else
            throw new NotImplementedException();
#endif
        }
        #endregion #region ITextureCubeStrategy



        internal static TextureTarget GetGLCubeFace(CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.PositiveX:
                    return TextureTarget.TextureCubeMapPositiveX;
                case CubeMapFace.NegativeX:
                    return TextureTarget.TextureCubeMapNegativeX;
                case CubeMapFace.PositiveY:
                    return TextureTarget.TextureCubeMapPositiveY;
                case CubeMapFace.NegativeY:
                    return TextureTarget.TextureCubeMapNegativeY;
                case CubeMapFace.PositiveZ:
                    return TextureTarget.TextureCubeMapPositiveZ;
                case CubeMapFace.NegativeZ:
                    return TextureTarget.TextureCubeMapNegativeZ;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
