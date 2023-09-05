// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;

#if OPENGL
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using PixelFormat = MonoGame.OpenGL.PixelFormat;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstructTexture2D(int width, int height, bool mipMap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            GetTextureStrategy<ConcreteTexture>()._glTarget = TextureTarget.Texture2D;
            ConcreteTexture.ToGLSurfaceFormat(format, GraphicsDevice, out GetTextureStrategy<ConcreteTexture>()._glInternalFormat, out GetTextureStrategy<ConcreteTexture>()._glFormat, out GetTextureStrategy<ConcreteTexture>()._glType);

            Threading.EnsureUIThread();
            {
                CreateGLTexture2D();

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

        private IntPtr PlatformGetSharedHandle()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture2D.SetData<T>(level, data, startIndex, elementCount);
            //TODO: move code to _strategyTexture2D.SetData<T>(...)

            Threading.EnsureUIThread();

            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                // Store the current bound texture.
                int prevTexture = GetBoundTexture2D();

                System.Diagnostics.Debug.Assert(GetTextureStrategy<ConcreteTexture>()._glTexture < 0);
                if (prevTexture != GetTextureStrategy<ConcreteTexture>()._glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(this.Format.GetSize(), 8));

                if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexImage2D(
                        TextureTarget.Texture2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, 0, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexImage2D(
                        TextureTarget.Texture2D, level, GetTextureStrategy<ConcreteTexture>()._glInternalFormat, w, h, 0, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, dataPtr);
                }
                GraphicsExtensions.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();
#endif
                // Restore the bound texture.
                if (prevTexture != GetTextureStrategy<ConcreteTexture>()._glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture2D.SetData<T>(level, arraySlice, checkedRect, data, startIndex, elementCount);
            //TODO: move code to _strategyTexture2D.SetData<T>(...)

            Threading.EnsureUIThread();

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                // Store the current bound texture.
                int prevTexture = GetBoundTexture2D();

                System.Diagnostics.Debug.Assert(GetTextureStrategy<ConcreteTexture>()._glTexture < 0);
                if (prevTexture != GetTextureStrategy<ConcreteTexture>()._glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(this.Format.GetSize(), 8));

                if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexSubImage2D(
                        TextureTarget.Texture2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        GetTextureStrategy<ConcreteTexture>()._glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexSubImage2D(
                        TextureTarget.Texture2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, dataPtr);
                }
                GraphicsExtensions.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();
#endif
                // Restore the bound texture.
                if (prevTexture != GetTextureStrategy<ConcreteTexture>()._glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture2D.GetData<T>(level, arraySlice, checkedRect, data, startIndex, elementCount);
            //TODO: move code to _strategyTexture2D.GetData<T>(...)

            Threading.EnsureUIThread();

#if GLES
            // TODO: check for non renderable formats (formats that can't be attached to FBO)

            int framebufferId = 0;
            framebufferId = GL.GenFramebuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GetTextureStrategy<ConcreteTexture>()._glTexture, 0);
            GraphicsExtensions.CheckGLError();

            GL.ReadPixels(checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, data);
            GraphicsExtensions.CheckGLError();
            GL.DeleteFramebuffer(framebufferId);
#else
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.Texture2D, GetTextureStrategy<ConcreteTexture>()._glTexture);
            GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSizeInByte, 8));

            if (GetTextureStrategy<ConcreteTexture>()._glFormat == GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                int pixelToT = Format.GetSize() / tSizeInByte;
                int tFullWidth = Math.Max(this.Width >> level, 1) / 4 * pixelToT;
                T[] temp = new T[Math.Max(this.Height >> level, 1) / 4 * tFullWidth];
                GL.GetCompressedTexImage(TextureTarget.Texture2D, level, temp);
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
                int tFullWidth = Math.Max(this.Width >> level, 1) * Format.GetSize() / tSizeInByte;
                T[] temp = new T[Math.Max(this.Height >> level, 1) * tFullWidth];
                GL.GetTexImage(TextureTarget.Texture2D, level, GetTextureStrategy<ConcreteTexture>()._glFormat, GetTextureStrategy<ConcreteTexture>()._glType, temp);
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
#endif
        }

        private void CreateGLTexture2D()
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
            if (GraphicsDevice.Strategy.Capabilities.SupportsTextureMaxLevel)
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

        private static int GetBoundTexture2D()
        {
            int prevTexture = 0;
            GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
            GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");
            return prevTexture;
        }

    }
}
