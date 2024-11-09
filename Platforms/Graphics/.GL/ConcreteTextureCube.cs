// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Utilities;
using GLPixelFormat = Microsoft.Xna.Platform.Graphics.OpenGL.PixelFormat;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;


        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format,
                                     bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            this.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public unsafe void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {

            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            {
                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                int elementSizeInBytes = sizeof(T);
                fixed (T* pData = &data[0])
                {
                    IntPtr dataPtr = (IntPtr)pData;
                    dataPtr = dataPtr + startIndex * elementSizeInBytes;

                    ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                    GL.ActiveTexture(TextureUnit.Texture0 + 0);
                    GL.CheckGLError();
                    GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
                    GL.CheckGLError();

                    TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
                    if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                            _glInternalFormat, elementCount * elementSizeInBytes, dataPtr);
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(
                            target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, dataPtr);
                        GL.CheckGLError();
                    }
                }
            }
        }

        public unsafe void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

#if OPENGL && DESKTOPGL
            TextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
            // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
            int fSize = this.Format.GetSize();
            int w = Math.Max(this.Size >> level, 1);
            int h = Math.Max(this.Size >> level, 1);
            int TsizeInBytes = sizeof(T);

            ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.CheckGLError();
            GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);

            fixed (T* pData = &data[0])
            {
                IntPtr dataPtr = (IntPtr)pData;
                dataPtr = dataPtr + startIndex * TsizeInBytes;

                if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    w = w / 4;
                    h = h / 4;
                    int bytes = w * h * fSize;
                    IntPtr pTemp = Marshal.AllocHGlobal(bytes);
                    try
                    {
                        GL.GetCompressedTexImage(target, level, pTemp);
                        GL.CheckGLError();

                        IntPtr tempPtr = (IntPtr)pTemp;
                        tempPtr = tempPtr + checkedRect.X / 4 * fSize + checkedRect.Top / 4 * w * fSize;
                        int fWidthSize = w * fSize;
                        int tRectWidthSize = checkedRect.Width / 4 * fSize;
                        int rowCount = checkedRect.Height / 4;
                        for (int r = 0; r < rowCount; r++)
                        {
                            MemCopyHelper.MemoryCopy(
                                tempPtr + r * fWidthSize,
                                dataPtr + r * tRectWidthSize,
                                tRectWidthSize);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pTemp);
                    }
                }
                else
                {
                    if (level == 0
                    && checkedRect.X == 0 && checkedRect.Y == 0
                    && checkedRect.Width == this.Size && checkedRect.Height == this.Size
                    && startIndex == 0 && elementCount == data.Length)
                    {
                        GL.GetTexImage(target, level, _glFormat, _glType, dataPtr);
                        GL.CheckGLError();
                    }
                    else
                    {
                        int bytes = w * h * fSize;
                        IntPtr pTemp = Marshal.AllocHGlobal(bytes);
                        try
                        {
                            GL.GetTexImage(target, level, _glFormat, _glType, pTemp);
                            GL.CheckGLError();

                            IntPtr tempPtr = (IntPtr)pTemp;
                            tempPtr = tempPtr + checkedRect.X * fSize + checkedRect.Top * w * fSize;
                            int fWidthSize = w * fSize;
                            int tRectWidthSize = checkedRect.Width * fSize;
                            int rowCount = checkedRect.Height;
                            for (int r = 0; r < rowCount; r++)
                            {
                                MemCopyHelper.MemoryCopy(
                                    tempPtr + r * fWidthSize,
                                    dataPtr + r * tRectWidthSize,
                                    tRectWidthSize);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pTemp);
                        }
                    }
                }
            }
#else
            throw new NotImplementedException();
#endif
        }

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            // round x and y down to next multiple of four; width and height up to next multiple of four
            int roundedWidth = (rect.Width + 3) & ~0x3;
            int roundedHeight = (rect.Height + 3) & ~0x3;
            // OpenGL only: The last two mip levels require the width and height to be passed
            // as 2x2 and 1x1, but there needs to be enough data passed to occupy a 4x4 block.
            checkedRect = new Rectangle(rect.X & ~0x3, rect.Y & ~0x3,
                                        (rect.Width < 4 && textureBounds.Width < 4) ? textureBounds.Width : roundedWidth,
                                        (rect.Height < 4 && textureBounds.Height < 4) ? textureBounds.Height : roundedHeight);
            return (roundedWidth * roundedHeight * fSize / 16);
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

            contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                _glTexture = GL.GenTexture();
                GL.CheckGLError();

                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(TextureUnit.Texture0 + 0);
                GL.CheckGLError();
                GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
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
