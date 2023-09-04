// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        private void PlatformConstructTexture2D(int width, int height, bool mipMap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = WebGLTextureTarget.TEXTURE_2D;
            ConcreteTexture.ToGLSurfaceFormat(format, GraphicsDevice,
                out GetTextureStrategy<ConcreteTexture>()._glInternalFormat,
                out GetTextureStrategy<ConcreteTexture>()._glFormat,
                out GetTextureStrategy<ConcreteTexture>()._glType,
                out GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture);

            {
                GenerateGLTextureIfRequired();

                var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                int w = width;
                int h = height;
                int level = 0;
                while (true)
                {
                    if (GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture)
                    {
                        int imageSize = 0;
                        // PVRTC has explicit calculations for imageSize
                        // https://www.khronos.org/registry/OpenGL/extensions/IMG/IMG_texture_compression_pvrtc.txt
                        if (format == SurfaceFormat.RgbPvrtc2Bpp || format == SurfaceFormat.RgbaPvrtc2Bpp)
                        {
                            imageSize = (Math.Max(w, 16) * Math.Max(h, 8) * 2 + 7) / 8;
                        }
                        else if (format == SurfaceFormat.RgbPvrtc4Bpp || format == SurfaceFormat.RgbaPvrtc4Bpp)
                        {
                            imageSize = (Math.Max(w, 8) * Math.Max(h, 8) * 4 + 7) / 8;
                        }
                        else
                        {
                            int blockSize = format.GetSize();
                            int blockWidth, blockHeight;
                            format.GetBlockSize(out blockWidth, out blockHeight);
                            int wBlocks = (w + (blockWidth - 1)) / blockWidth;
                            int hBlocks = (h + (blockHeight - 1)) / blockHeight;
                            imageSize = wBlocks * hBlocks * blockSize;
                        }
                        var data = new byte[imageSize]; // WebGL CompressedTexImage2D requires data.
                        GL.CompressedTexImage2D(WebGLTextureTarget.TEXTURE_2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, data);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType);
                        GraphicsExtensions.CheckGLError();
                    }

                    if ((w == 1 && h == 1) || !mipMap)
                        break;
                    if (w > 1)
                        w = w / 2;
                    if (h > 1)
                        h = h / 2;
                    ++level;
                }
            }
        }

        private IntPtr PlatformGetSharedHandle()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            var startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0 && !GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture)
                throw new NotImplementedException("startIndex");

            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
            GraphicsExtensions.CheckGLError();

            GenerateGLTextureIfRequired();
            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));
            GraphicsExtensions.CheckGLError();

            if (GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture)
            {
                GL.CompressedTexImage2D(
                        WebGLTextureTarget.TEXTURE_2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, data, startIndex, elementCount);
            }
            else
            {
                GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, data);
            }
            GraphicsExtensions.CheckGLError();

            //GL.Finish();
            //GraphicsExtensions.CheckGLError();
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            var startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0)
                throw new NotImplementedException("startIndex");

            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
            GraphicsExtensions.CheckGLError();

            GenerateGLTextureIfRequired();
            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));

            if (GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture)
            {
                throw new NotImplementedException();
            }
            else
            {
                GL.TexSubImage2D(
                    WebGLTextureTarget.TEXTURE_2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                    GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, data);
            }
            GraphicsExtensions.CheckGLError();

            //GL.Finish();
            //GraphicsExtensions.CheckGLError();
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void GenerateGLTextureIfRequired()
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            if (GetTextureStrategy<ConcreteTexture>()._glTexture == null)
            {
                GetTextureStrategy<ConcreteTexture>()._glTexture = GL.CreateTexture();
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                var wrap = WebGLTexParam.REPEAT;
                if (((this.Width & (this.Width - 1)) != 0) || ((this.Height & (this.Height - 1)) != 0))
                    wrap = WebGLTexParam.CLAMP_TO_EDGE;

                GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MIN_FILTER,
                    (this.LevelCount > 1) ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MAG_FILTER,
                    WebGLTexParam.LINEAR);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_S, wrap);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_T, wrap);
                GraphicsExtensions.CheckGLError();

                // Set mipMap levels
                //GL2.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGL2TexParamName.TEXTURE_BASE_LEVEL, 0);
                //GraphicsExtensions.CheckGLError();
                if (GraphicsDevice.Strategy.Capabilities.SupportsTextureMaxLevel)
                {
                    if (this.LevelCount > 0)
                    {
                        throw new NotImplementedException();
                        // GL2.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGL2TexParamName.TEXTURE_MAX_LEVEL, _levelCount - 1);
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // GL2.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGL2TexParamName.TEXTURE_MAX_LEVEL, 1000);
                    }
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

    }
}

