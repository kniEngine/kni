// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

#if OPENGL
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using PixelFormat = MonoGame.OpenGL.PixelFormat;
#endif

#if ANDROID && OPENGL
using Android.Graphics;
#endif

#if IOS || TVOS
using UIKit;
using CoreGraphics;
using Foundation;
using System.Drawing;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstructTexture2D(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            this._glTarget = TextureTarget.Texture2D;
            ToGLSurfaceFormat(format, GraphicsDevice, out _glInternalFormat, out _glFormat, out _glType);

            Threading.EnsureUIThread();
            {
                GenerateGLTextureIfRequired();
                int w = width;
                int h = height;
                int level = 0;
                while (true)
                {
                    if (_glFormat == GLPixelFormat.CompressedTextureFormats)
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
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, imageSize, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, _glFormat, _glType, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
                    }

                    if ((w == 1 && h == 1) || !mipmap)
                        break;
                    if (w > 1)
                        w = w / 2;
                    if (h > 1)
                        h = h / 2;
                    ++level;
                }
            }
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureUIThread();

            int w, h;
            GetSizeForLevel(Width, Height, level, out w, out h);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                // Store the current bound texture.
                int prevTexture = GetBoundTexture2D();

                if (prevTexture != _glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, _glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GenerateGLTextureIfRequired();
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(_format.GetSize(), 8));

                if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexImage2D(
                        TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexImage2D(
                        TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, _glFormat, _glType, dataPtr);
                }
                GraphicsExtensions.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();
#endif
                // Restore the bound texture.
                if (prevTexture != _glTexture)
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

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
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
                if (prevTexture != _glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, _glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GenerateGLTextureIfRequired();
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(_format.GetSize(), 8));

                if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexSubImage2D(
                        TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height,
                        _glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexSubImage2D(
                        TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height,
                        _glFormat, _glType, dataPtr);
                }
                GraphicsExtensions.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();
#endif
                // Restore the bound texture.
                if (prevTexture != _glTexture)
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

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureUIThread();

#if GLES
            // TODO: check for non renderable formats (formats that can't be attached to FBO)

            int framebufferId = 0;
            framebufferId = GL.GenFramebuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, this._glTexture, 0);
            GraphicsExtensions.CheckGLError();

            GL.ReadPixels(rect.X, rect.Y, rect.Width, rect.Height, this._glFormat, this._glType, data);
            GraphicsExtensions.CheckGLError();
            GL.DeleteFramebuffer(framebufferId);
#else
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.Texture2D, this._glTexture);
            GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSizeInByte, 8));

            if (_glFormat == GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                int pixelToT = Format.GetSize() / tSizeInByte;
                int tFullWidth = Math.Max(this._width >> level, 1) / 4 * pixelToT;
                T[] temp = new T[Math.Max(this._height >> level, 1) / 4 * tFullWidth];
                GL.GetCompressedTexImage(TextureTarget.Texture2D, level, temp);
                GraphicsExtensions.CheckGLError();

                int rowCount = rect.Height / 4;
                int tRectWidth = rect.Width / 4 * Format.GetSize() / tSizeInByte;
                for (int r = 0; r < rowCount; r++)
                {
                    int tempStart = rect.X / 4 * pixelToT + (rect.Top / 4 + r) * tFullWidth;
                    int dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
            else
            {
                // we need to convert from our format size to the size of T here
                int tFullWidth = Math.Max(this._width >> level, 1) * Format.GetSize() / tSizeInByte;
                T[] temp = new T[Math.Max(this._height >> level, 1) * tFullWidth];
                GL.GetTexImage(TextureTarget.Texture2D, level, _glFormat, _glType, temp);
                GraphicsExtensions.CheckGLError();

                int pixelToT = Format.GetSize() / tSizeInByte;
                int rowCount = rect.Height;
                int tRectWidth = rect.Width * pixelToT;
                for (int r = 0; r < rowCount; r++)
                {
                    int tempStart = rect.X * pixelToT + (r + rect.Top) * tFullWidth;
                    int dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
#endif
        }

#if IOS || TVOS
        [CLSCompliant(false)]
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, UIImage uiImage)
        {
            return PlatformFromStream(graphicsDevice, uiImage.CGImage);
        }
#elif ANDROID
        [CLSCompliant(false)]
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Bitmap bitmap)
        {
            return PlatformFromStream(graphicsDevice, bitmap);
        }

        [CLSCompliant(false)]
        public void Reload(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                using (Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    Canvas canvas = new Canvas(imagePadded);
                    canvas.DrawARGB(0, 0, 0, 0);
                    canvas.DrawBitmap(image, 0, 0, null);
                    imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
                    imagePadded.Recycle();
                }
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }

            image.Recycle();

            this.SetData<int>(pixels);
        }
#endif

#if IOS || TVOS
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, CGImage cgImage)
        {
            int width = (int)cgImage.Width;
            int height = (int)cgImage.Height;

            byte[] data = new byte[width * height * 4];

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace, CGBitmapFlags.PremultipliedLast);
            bitmapContext.DrawImage(new RectangleF(0, 0, width, height), cgImage);
            bitmapContext.Dispose();
            colorSpace.Dispose();

            Texture2D texture = null;
            Threading.EnsureUIThread();
            {
                texture = new Texture2D(graphicsDevice, (int)width, (int)height, false, SurfaceFormat.Color);
                texture.SetData(data);
            }

            return texture;
        }
#elif ANDROID
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                using (Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    Canvas canvas = new Canvas(imagePadded);
                    canvas.DrawARGB(0, 0, 0, 0);
                    canvas.DrawBitmap(image, 0, 0, null);
                    imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
                    imagePadded.Recycle();
                }
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }
            image.Recycle();

            // Convert from ARGB to ABGR
            ConvertToABGR(height, width, pixels);

            Texture2D texture = null;
            Threading.EnsureUIThread();
            {
                texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
                texture.SetData<int>(pixels);
            }

            return texture;
        }
#endif

        private void FillTextureFromStream(Stream stream)
        {
#if ANDROID
            using (Bitmap image = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,
            }))
            {
                int width = image.Width;
                int height = image.Height;

                int[] pixels = new int[width * height];
                image.GetPixels(pixels, 0, width, 0, 0, width, height);

                // Convert from ARGB to ABGR
                ConvertToABGR(height, width, pixels);

                this.SetData<int>(pixels);
                image.Recycle();
            }
#endif
        }

        // This method allows games that use Texture2D.FromStream
        // to reload their textures after the GL context is lost.
        private void PlatformReload(Stream textureStream)
        {
            int prev = GetBoundTexture2D();

            GenerateGLTextureIfRequired();
            FillTextureFromStream(textureStream);

            GL.BindTexture(TextureTarget.Texture2D, prev);
        }

        private void GenerateGLTextureIfRequired()
        {
            if (this._glTexture < 0)
            {
                this._glTexture = GL.GenTexture();
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                TextureWrapMode wrap = TextureWrapMode.Repeat;
                if (((_width & (_width - 1)) != 0) || ((_height & (_height - 1)) != 0))
                    wrap = TextureWrapMode.ClampToEdge;

                GL.BindTexture(TextureTarget.Texture2D, this._glTexture);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (_levelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(
                    TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
                GraphicsExtensions.CheckGLError();

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
                GraphicsExtensions.CheckGLError();

                // Set mipmap levels
#if !GLES
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
#endif
                GraphicsExtensions.CheckGLError();
                if (GraphicsDevice.Strategy.Capabilities.SupportsTextureMaxLevel)
                {
                    if (_levelCount > 0)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, _levelCount - 1);
                    }
                    else
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, 1000);
                    }
                    GraphicsExtensions.CheckGLError();
                }
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
