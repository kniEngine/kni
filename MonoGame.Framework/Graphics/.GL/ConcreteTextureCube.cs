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
using Microsoft.Xna.Platform.Graphics.OpenGL;
using GLPixelFormat = Microsoft.Xna.Platform.Graphics.OpenGL.PixelFormat;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;


        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format,
                                     bool isRenderTarget)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            this.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {

            Threading.EnsureUIThread();

            {
                var GL = OGL.Current;

                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                // Use try..finally to make sure dataHandle is freed in case of an error
                try
                {
                    int startBytes = startIndex * elementSizeInByte;
                    IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    this.GraphicsDevice.CurrentContext.Textures.Strategy.Dirty(0);
                    GL.ActiveTexture(TextureUnit.Texture0 + 0);
                    GL.CheckGLError();
                    GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
                    GL.CheckGLError();

                    TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
                    if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                            _glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, dataPtr);
                        GL.CheckGLError();
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

            var GL = OGL.Current;

#if OPENGL && DESKTOPGL
            TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();

            this.GraphicsDevice.CurrentContext.Textures.Strategy.Dirty(0);
            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.CheckGLError();
            GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);

            if (_glFormat == GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                int pixelToT = Format.GetSize() / tSizeInByte;
                int tFullWidth = Math.Max(this.Size >> level, 1) / 4 * pixelToT;
                T[] temp = new T[Math.Max(this.Size >> level, 1) / 4 * tFullWidth];
                GL.GetCompressedTexImage(target, level, temp);
                GL.CheckGLError();

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
                GL.CheckGLError();

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
        #endregion ITextureCubeStrategy



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


        internal void PlatformConstructTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            _glTarget = TextureTarget.TextureCubeMap;

            Threading.EnsureUIThread();
            {
                var GL = OGL.Current;

                _glTexture = GL.GenTexture();
                GL.CheckGLError();

                this.GraphicsDevice.CurrentContext.Textures.Strategy.Dirty(0);
                GL.ActiveTexture(TextureUnit.Texture0 + 0);
                GL.CheckGLError();
                GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
                GL.CheckGLError();

                GL.TexParameter(
                    TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                    mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GL.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.CheckGLError();

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy,
                    out _glInternalFormat,
                    out _glFormat,
                    out _glType);

                for (int i = 0; i < 6; i++)
                {
                    TextureTarget target = ConcreteTextureCube.GetGLCubeFace((CubeMapFace)i);

                    if (_glFormat == GLPixelFormat.CompressedTextureFormats)
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
                        GL.CompressedTexImage2D(target, 0, _glInternalFormat, size, size, 0, imageSize, IntPtr.Zero);
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(target, 0, _glInternalFormat, size, size, 0, _glFormat, _glType, IntPtr.Zero);
                        GL.CheckGLError();
                    }
                }

                if (mipMap)
                {
                    System.Diagnostics.Debug.Assert(TextureTarget.TextureCubeMap == _glTarget);
#if IOS || TVOS || ANDROID
                    GL.GenerateMipmap(TextureTarget.TextureCubeMap);
                    GL.CheckGLError();
#else
                    GL.GenerateMipmap(_glTarget);
                    GL.CheckGLError();
                    // This updates the mipmaps after a change in the base texture
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)Bool.True);
#endif
                }
            }
        }

    }
}
