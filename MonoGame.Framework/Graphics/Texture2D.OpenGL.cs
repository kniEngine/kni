// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

#if OPENGL
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = TextureTarget.Texture2D;
            ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy.Context.DeviceStrategy, out GetTextureStrategy<ConcreteTexture>()._glInternalFormat, out GetTextureStrategy<ConcreteTexture>()._glFormat, out GetTextureStrategy<ConcreteTexture>()._glType);

            Threading.EnsureUIThread();
            {
                CreateGLTexture2D(contextStrategy);

                int w = width;
                int h = height;
                int level = 0;
                while (true)
                {
                    if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
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
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, 0, imageSize, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, 0, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, IntPtr.Zero);
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
            System.Diagnostics.Debug.Assert(GetTextureStrategy<ConcreteTexture>()._glTexture < 0);

            GetTextureStrategy<ConcreteTexture>()._glTexture = GL.GenTexture();
            GraphicsExtensions.CheckGLError();

            // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
            // dimension is not a power of two.
            TextureWrapMode wrap = TextureWrapMode.Repeat;
            if (((this.Width & (this.Width - 1)) != 0) || ((this.Height & (this.Height - 1)) != 0))
                wrap = TextureWrapMode.ClampToEdge;

            GL.BindTexture(TextureTarget.Texture2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
            GraphicsExtensions.CheckGLError();

            GL.TexParameter(
                TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (this.LevelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GraphicsExtensions.CheckGLError();

            GL.TexParameter(
                TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GraphicsExtensions.CheckGLError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
            GraphicsExtensions.CheckGLError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
            GraphicsExtensions.CheckGLError();

            // Set mipMap levels
#if !GLES
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
#endif
            GraphicsExtensions.CheckGLError();
            if (contextStrategy.Context.DeviceStrategy.Capabilities.SupportsTextureMaxLevel)
            {
                if (this.LevelCount > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, this.LevelCount - 1);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, 1000);
                }
                GraphicsExtensions.CheckGLError();
            }
        }

    }
}
