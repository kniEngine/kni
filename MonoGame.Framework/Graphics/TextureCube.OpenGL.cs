// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class TextureCube
    {
        private void PlatformConstructTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = TextureTarget.TextureCubeMap;

            Threading.EnsureUIThread();
            {
                GetTextureStrategy<ConcreteTexture>()._glTexture = GL.GenTexture();
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(TextureTarget.TextureCubeMap, GetTextureStrategy<ConcreteTexture>()._glTexture);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                    mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GraphicsExtensions.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, GraphicsDevice, out GetTextureStrategy<ConcreteTexture>()._glInternalFormat, out GetTextureStrategy<ConcreteTexture>()._glFormat, out GetTextureStrategy<ConcreteTexture>()._glType);

                for (int i = 0; i < 6; i++)
                {
                    TextureTarget target = GetGLCubeFace((CubeMapFace)i);

                    if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        int imageSize = 0;
                        switch (format)
                        {
                            case SurfaceFormat.RgbPvrtc2Bpp:
                            case SurfaceFormat.RgbaPvrtc2Bpp:
                                imageSize = (Math.Max(size, 16) * Math.Max(size, 8) * 2 + 7) / 8;
                                break;
                            case SurfaceFormat.RgbPvrtc4Bpp:
                            case SurfaceFormat.RgbaPvrtc4Bpp:
                                imageSize = (Math.Max(size, 8) * Math.Max(size, 8) * 4 + 7) / 8;
                                break;
                            case SurfaceFormat.Dxt1:
                            case SurfaceFormat.Dxt1a:
                            case SurfaceFormat.Dxt1SRgb:
                            case SurfaceFormat.Dxt3:
                            case SurfaceFormat.Dxt3SRgb:
                            case SurfaceFormat.Dxt5:
                            case SurfaceFormat.Dxt5SRgb:
                            case SurfaceFormat.RgbEtc1:
                            case SurfaceFormat.Rgb8Etc2:
                            case SurfaceFormat.Srgb8Etc2:
                            case SurfaceFormat.Rgb8A1Etc2:
                            case SurfaceFormat.Srgb8A1Etc2:
                            case SurfaceFormat.Rgba8Etc2:
                            case SurfaceFormat.SRgb8A8Etc2:
                            case SurfaceFormat.RgbaAtcExplicitAlpha:
                            case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                                imageSize = (size + 3) / 4 * ((size + 3) / 4) * format.GetSize();
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        GL.CompressedTexImage2D(target, 0, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, size, size, 0, imageSize, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(target, 0, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, size, size, 0, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                if (mipMap)
                {
                    System.Diagnostics.Debug.Assert(TextureTarget.TextureCubeMap == GetTextureStrategy<ConcreteTexture>()._glTarget);
#if IOS || TVOS || ANDROID
                    GL.GenerateMipmap(TextureTarget.TextureCubeMap);
                    GraphicsExtensions.CheckGLError();
#else
                    GL.GenerateMipmap(GetTextureStrategy<ConcreteTexture>()._glTarget);
                    GraphicsExtensions.CheckGLError();
                    // This updates the mipmaps after a change in the base texture
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)Bool.True);
#endif
                }
            }
        }

        private void PlatformGetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureUIThread();

#if OPENGL && DESKTOPGL
            TextureTarget target = GetGLCubeFace(face);
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.TextureCubeMap, GetTextureStrategy<ConcreteTexture>()._glTexture);

            if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
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
                GL.GetTexImage(target, level, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, temp);
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

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
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

                    GL.BindTexture(TextureTarget.TextureCubeMap, GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GraphicsExtensions.CheckGLError();

                    TextureTarget target = GetGLCubeFace(face);
                    if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                            GetTextureStrategy<ConcreteTexture>()._glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        private TextureTarget GetGLCubeFace(CubeMapFace face)
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

