// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;


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
            var GL = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            int startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0 && !_glIsCompressedTexture)
                throw new NotImplementedException("startIndex");

            System.Diagnostics.Debug.Assert(_glTexture != null);
            base.GraphicsDeviceStrategy.CurrentContext.Textures.Strategy.Dirty(0);
            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
            GL.CheckGLError();
            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GL.CheckGLError();

            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));
            GL.CheckGLError();

            if (_glIsCompressedTexture)
            {
                GL.CompressedTexImage2D(
                        WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, data, startIndex, elementCount);
            }
            else
            {
                GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, _glFormat, _glType, data);
            }
            GL.CheckGLError();

            //GL.Finish();
            //GL.CheckGLError();
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            var GL = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            int startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0)
                throw new NotImplementedException("startIndex");

            System.Diagnostics.Debug.Assert(_glTexture != null);
            base.GraphicsDeviceStrategy.CurrentContext.Textures.Strategy.Dirty(0);
            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
            GL.CheckGLError();
            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GL.CheckGLError();

            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));

            if (_glIsCompressedTexture)
            {
                throw new NotImplementedException();
            }
            else
            {
                GL.TexSubImage2D(
                    WebGLTextureTarget.TEXTURE_2D, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                    _glFormat, _glType, data);
            }
            GL.CheckGLError();

            //GL.Finish();
            //GL.CheckGLError();
        }

        public void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            var GL = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            // TODO: check for non renderable formats (formats that can't be attached to FBO)

            WebGLFramebuffer glFramebuffer;
            glFramebuffer = GL.CreateFramebuffer();
            GL.CheckGLError();
            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
            GL.CheckGLError();
            GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0, WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GL.CheckGLError();

            GL.ReadPixels(checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, data);
            GL.CheckGLError();
            glFramebuffer.Dispose();
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
            checkedRect = new Rectangle(rect.X & ~blockWidthMinusOne, rect.Y & ~blockHeightMinusOne,
#if OPENGL
                    // OpenGL only: The last two mip levels require the width and height to be
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy
                    // a full block.
                    (rect.Width < blockWidth && textureBounds.Width < blockWidth) ? textureBounds.Width : roundedWidth,
                    (rect.Height < blockHeight && textureBounds.Height < blockHeight) ? textureBounds.Height : roundedHeight);
#else
                                        roundedWidth, roundedHeight);
#endif
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
            _glTarget = WebGLTextureTarget.TEXTURE_2D;
            ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy,
                out _glInternalFormat,
                out _glFormat,
                out _glType,
                out _glIsCompressedTexture);

            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                CreateGLTexture2D(contextStrategy);

                int w = width;
                int h = height;
                int level = 0;
                while (true)
                {
                    if (_glIsCompressedTexture)
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
                        byte[] data = new byte[imageSize]; // WebGL CompressedTexImage2D requires data.
                        GL.CompressedTexImage2D(WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, data);
                        GL.CheckGLError();
                    }
                    else
                    {
                        GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, _glFormat, _glType);
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
            System.Diagnostics.Debug.Assert(_glTexture == null);

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _glTexture = GL.CreateTexture();
            GL.CheckGLError();

            // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
            // dimension is not a power of two.
            WebGLTexParam wrap = WebGLTexParam.REPEAT;
            if (((this.Width & (this.Width - 1)) != 0) || ((this.Height & (this.Height - 1)) != 0))
                wrap = WebGLTexParam.CLAMP_TO_EDGE;

            base.GraphicsDeviceStrategy.CurrentContext.Textures.Strategy.Dirty(0);
            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
            GL.CheckGLError();
            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GL.CheckGLError();

            GL.TexParameter(
                WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MIN_FILTER,
                (this.LevelCount > 1) ? WebGLTexParam.LINEAR_MIPMAP_LINEAR : WebGLTexParam.LINEAR);
            GL.CheckGLError();

            GL.TexParameter(
                WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MAG_FILTER,
                WebGLTexParam.LINEAR);
            GL.CheckGLError();

            GL.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_S, wrap);
            GL.CheckGLError();

            GL.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_T, wrap);
            GL.CheckGLError();

            // Set mipMap levels
            //GL2.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGL2TexParamName.TEXTURE_BASE_LEVEL, 0);
            //GL.CheckGLError();
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
                GL.CheckGLError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this, base.GraphicsDeviceStrategy.CurrentContext.Strategy);

            base.Dispose(disposing);
        }
    }
}
