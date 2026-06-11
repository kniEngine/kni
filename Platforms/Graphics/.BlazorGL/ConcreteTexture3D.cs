// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;


        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format,
                                   bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this.PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            TextureHelpers.GetSizeForLevel(Width, Height, Depth, level, out int w, out int h, out int d);

            System.Diagnostics.Debug.Assert(_glTexture != null);
            ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
            GL.CheckGLError();
            GL.BindTexture(WebGLTextureTarget.TEXTURE_3D, _glTexture);
            GL.CheckGLError();

            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));
            GL.CheckGLError();

            if (_glIsCompressedTexture)
            {
                // TODO: Requires ASTC and/or BPTC support adding before CompressedTexSubImage3D can be implemented.
                throw new NotImplementedException("Texture3D does not yet support compressed formats on this platform.");

                ((IWebGL2RenderingContext)GL).CompressedTexSubImage3D(
                    WebGLTextureTarget.TEXTURE_3D, level, left, top, front, w, h, d, _glFormat, data, startIndex, elementCount);
                GL.CheckGLError();
            }
            else
            {
                ((IWebGL2RenderingContext)GL).TexSubImage3D(
                    WebGLTextureTarget.TEXTURE_3D, level, left, top, front, w, h, d, _glFormat, _glType, data, startIndex, elementCount);
                GL.CheckGLError();
            }
        }

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
            throw new NotImplementedException();
        }

        public int GetCompressedDataByteSize(int fSize, int left, int top, int right, int bottom, int front, int back,
                                             int textureBoundsWidth, int textureBoundsHeight, int textureBoundsDepth,
                                             out int checkedLeft, out int checkedTop, out int checkedRight, out int checkedBottom,
                                             out int checkedFront, out int checkedBack)
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;
            Format.GetBlockSize(out int blockWidth, out int blockHeight);
            int blockWidthMinusOne = blockWidth - 1;
            int blockHeightMinusOne = blockHeight - 1;
            // round x and y down to next multiple of block size; width and height up to next multiple of block size
            int roundedWidth = (width + blockWidthMinusOne) & ~blockWidthMinusOne;
            int roundedHeight = (height + blockHeightMinusOne) & ~blockHeightMinusOne;
            checkedLeft = left & ~blockWidthMinusOne;
            checkedTop = top & ~blockHeightMinusOne;
            // The last two mip levels require the width and height to be passed
            // as 2x2 and 1x1, but there needs to be enough data passed to occupy a full block.
            checkedRight = (width < blockWidth && textureBoundsWidth < blockWidth) ? textureBoundsWidth : checkedLeft + roundedWidth;
            checkedBottom = (height < blockHeight && textureBoundsHeight < blockHeight) ? textureBoundsHeight : checkedTop + roundedHeight;
            checkedFront = front;
            checkedBack = back;
            if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
            {
                return (Math.Max(checkedRight - checkedLeft, 16) * Math.Max(checkedBottom - checkedTop, 8) * 2 + 7) / 8 * depth;
            }
            else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
            {
                return (Math.Max(checkedRight - checkedLeft, 8) * Math.Max(checkedBottom - checkedTop, 8) * 4 + 7) / 8 * depth;
            }
            else
            {
                return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight) * depth;
            }
        }
        #endregion ITexture3DStrategy

        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            _glTarget = WebGLTextureTarget.TEXTURE_3D;

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            System.Diagnostics.Debug.Assert(_glTexture == null);
            _glTexture = GL.CreateTexture();
            GL.CheckGLError();

            ((IPlatformTextureCollection)contextStrategy.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
            GL.CheckGLError();
            GL.BindTexture(_glTarget, _glTexture);
            GL.CheckGLError();

            ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy,
                out _glInternalFormat,
                out _glFormat,
                out _glType,
                out _glIsCompressedTexture);

            int w = width;
            int h = height;
            int d = depth;
            int level = 0;
            while (true)
            {
                if (_glIsCompressedTexture)
                {
                    // TODO: Requires ASTC and/or BPTC support adding before CompressedTexImage3D can be implemented.
                    throw new NotImplementedException("Texture3D does not yet support compressed formats on this platform.");

                    int dataSize = GetCompressedDataByteSize(
                        format.GetSize(), 0, 0, w, h, 0, d, w, h, d,
                        out int checkedLeft, out int checkedTop, out int checkedRight, out int checkedBottom, out int checkedFront, out int checkedBack);
                    byte[] data = new byte[dataSize]; // WebGL CompressedTexImage3D requires data.
                    ((IWebGL2RenderingContext)GL).CompressedTexImage3D(
                        WebGLTextureTarget.TEXTURE_3D, level, _glInternalFormat, checkedRight, checkedBottom, checkedBack, data);
                    GL.CheckGLError();
                }
                else
                {
                    ((IWebGL2RenderingContext)GL).TexImage3D(_glTarget, level, _glInternalFormat, w, h, d, _glFormat, _glType);
                    GL.CheckGLError();
                }

                if ((w == 1 && h == 1 && d == 1) || !mipMap)
                    break;
                if (w > 1)
                    w = w / 2;
                if (h > 1)
                    h = h / 2;
                if (d > 1)
                    d = d / 2;
                ++level;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            base.Dispose(disposing);
        }

    }
}
