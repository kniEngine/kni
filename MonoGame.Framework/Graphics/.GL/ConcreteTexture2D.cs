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
    internal partial class ConcreteTexture2D : ConcreteTexture, ITexture2DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _arraySize;


        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared,
                                   bool isRenderTarget)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;

            this.PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }


        #region ITexture2DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int ArraySize { get { return _arraySize; } }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, this._width, this._height); }
        }

        public IntPtr GetSharedHandle()
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureMainThread();

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                System.Diagnostics.Debug.Assert(_glTexture >= 0);
                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(TextureUnit.Texture0 + 0);
                GL.CheckGLError();
                GL.BindTexture(TextureTarget.Texture2D, _glTexture);
                GL.CheckGLError();

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(this.Format.GetSize(), 8));

                if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexImage2D(
                        TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexImage2D(
                        TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0,_glFormat, _glType, dataPtr);
                }
                GL.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GL.CheckGLError();
#endif
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureMainThread();

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                System.Diagnostics.Debug.Assert(_glTexture >= 0);
                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(TextureUnit.Texture0 + 0);
                GL.CheckGLError();
                GL.BindTexture(TextureTarget.Texture2D, _glTexture);
                GL.CheckGLError();

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(this.Format.GetSize(), 8));

                if (_glFormat == GLPixelFormat.CompressedTextureFormats)
                {
                    GL.CompressedTexSubImage2D(
                        TextureTarget.Texture2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        _glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                }
                else
                {
                    GL.TexSubImage2D(
                        TextureTarget.Texture2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        _glFormat, _glType, dataPtr);
                }
                GL.CheckGLError();

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GL.CheckGLError();
#endif
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            Threading.EnsureMainThread();

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

#if GLES
            // TODO: check for non renderable formats (formats that can't be attached to FBO)

            int framebufferId = 0;
            framebufferId = GL.GenFramebuffer();
            GL.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GL.CheckGLError();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _glTexture, 0);
            GL.CheckGLError();

            GL.ReadPixels(checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, data);
            GL.CheckGLError();
            GL.DeleteFramebuffer(framebufferId);
#else
            int tSizeInByte = ReflectionHelpers.SizeOf<T>();

            ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.CheckGLError();
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSizeInByte, 8));

            if (_glFormat == GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                int pixelToT = Format.GetSize() / tSizeInByte;
                int tFullWidth = Math.Max(this.Width >> level, 1) / 4 * pixelToT;
                T[] temp = new T[Math.Max(this.Height >> level, 1) / 4 * tFullWidth];
                GL.GetCompressedTexImage(TextureTarget.Texture2D, level, temp);
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
                int tFullWidth = Math.Max(this.Width >> level, 1) * Format.GetSize() / tSizeInByte;
                T[] temp = new T[Math.Max(this.Height >> level, 1) * tFullWidth];
                GL.GetTexImage(TextureTarget.Texture2D, level, _glFormat, _glType, temp);
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
#endif
        }

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            int blockWidth, blockHeight;
            Format.GetBlockSize(out blockWidth, out blockHeight);
            int blockWidthMinusOne = blockWidth - 1;
            int blockHeightMinusOne = blockHeight - 1;
            // round x and y down to next multiple of block size; width and height up to next multiple of block size
            int roundedWidth = (rect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
            int roundedHeight = (rect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
            // OpenGL only: The last two mip levels require the width and height to be passed
            // as 2x2 and 1x1, but there needs to be enough data passed to occupy a full block.
            checkedRect = new Rectangle(rect.X & ~blockWidthMinusOne, rect.Y & ~blockHeightMinusOne,
                                        (rect.Width < blockWidth && textureBounds.Width < blockWidth) ? textureBounds.Width : roundedWidth,
                                        (rect.Height < blockHeight && textureBounds.Height < blockHeight) ? textureBounds.Height : roundedHeight);
            if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
            {
                return (Math.Max(checkedRect.Width, 16) * Math.Max(checkedRect.Height, 8) * 2 + 7) / 8;
            }
            else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
            {
                return (Math.Max(checkedRect.Width, 8) * Math.Max(checkedRect.Height, 8) * 4 + 7) / 8;
            }
            else
            {
                return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight);
            }
        }
        #endregion ITexture2DStrategy


        internal void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            _glTarget = TextureTarget.Texture2D;
            ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy,
                out _glInternalFormat,
                out _glFormat,
                out _glType);

            Threading.EnsureMainThread();
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                CreateGLTexture2D(contextStrategy);

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
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, level, _glInternalFormat, w, h, 0, _glFormat, _glType, IntPtr.Zero);
                        GL.CheckGLError();
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
            System.Diagnostics.Debug.Assert(_glTexture < 0);

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _glTexture = GL.GenTexture();
            GL.CheckGLError();

            // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
            // dimension is not a power of two.
            TextureWrapMode wrap = TextureWrapMode.Repeat;
            if (((this.Width & (this.Width - 1)) != 0) || ((this.Height & (this.Height - 1)) != 0))
                wrap = TextureWrapMode.ClampToEdge;

            ((IPlatformTextureCollection)contextStrategy.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.CheckGLError();
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.CheckGLError();

            GL.TexParameter(
                TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (this.LevelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GL.CheckGLError();

            GL.TexParameter(
                TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.CheckGLError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
            GL.CheckGLError();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
            GL.CheckGLError();

            // Set mipMap levels
#if !GLES
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.CheckGLError();
#endif
            if (contextStrategy.Context.DeviceStrategy.Capabilities.SupportsTextureMaxLevel)
            {
                if (this.LevelCount > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, ConcreteSamplerState.TextureParameterNameTextureMaxLevel, this.LevelCount - 1);
                    GL.CheckGLError();
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, ConcreteSamplerState.TextureParameterNameTextureMaxLevel, 1000);
                    GL.CheckGLError();
                }
            }
        }

    }
}
