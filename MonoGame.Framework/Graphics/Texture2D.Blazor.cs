// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        private void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = WebGLTextureTarget.TEXTURE_2D;
            ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy.Context.DeviceStrategy,
                out GetTextureStrategy<ConcreteTexture>()._glInternalFormat,
                out GetTextureStrategy<ConcreteTexture>()._glFormat,
                out GetTextureStrategy<ConcreteTexture>()._glType,
                out GetTextureStrategy<ConcreteTexture>()._glIsCompressedTexture);

            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                CreateGLTexture2D(contextStrategy);

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

        private void CreateGLTexture2D(GraphicsContextStrategy contextStrategy)
        {
            System.Diagnostics.Debug.Assert(GetTextureStrategy<ConcreteTexture>()._glTexture == null);

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

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
            if (contextStrategy.Context.DeviceStrategy.Capabilities.SupportsTextureMaxLevel)
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

