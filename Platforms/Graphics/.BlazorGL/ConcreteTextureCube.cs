// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;


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

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            {
                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
                GL.CheckGLError();
                GL.BindTexture(WebGLTextureTarget.TEXTURE_CUBE_MAP, _glTexture);
                GL.CheckGLError();

                WebGLTextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
                if (_glIsCompressedTexture)
                {
                    GL.CompressedTexSubImage2D(
                        target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        _glFormat, data, startIndex, elementCount);
                    GL.CheckGLError();
                }
                else
                {
                    GL.TexSubImage2D(target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height,
                        _glFormat, _glType, data, startIndex, elementCount);
                    GL.CheckGLError();
                }
            }
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            GraphicsContextStrategy contextStrategy = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy;
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            ValidateGetDataSurfaceFormat(Format, contextStrategy);

            WebGLTextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);

            WebGLFramebuffer glFramebuffer;
            glFramebuffer = GL.CreateFramebuffer();
            GL.CheckGLError();
            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
            GL.CheckGLError();
            GL.FramebufferTexture2D(
                WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0, target, _glTexture, level);
            GL.CheckGLError();

            GL.ReadPixels(checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, data);
            GL.CheckGLError();
            glFramebuffer.Dispose();
        }

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            Format.GetBlockSize(out int blockWidth, out int blockHeight);
            int blockWidthMinusOne = blockWidth - 1;
            int blockHeightMinusOne = blockHeight - 1;
            // round x and y down to next multiple of block size; width and height up to next multiple of block size
            int roundedWidth = (rect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
            int roundedHeight = (rect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
            // The last two mip levels require the width and height to be passed
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
        #endregion ITextureCubeStrategy


        internal static WebGLTextureTarget GetGLCubeFace(CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.PositiveX:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_X;
                case CubeMapFace.NegativeX:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_NEGATIVE_X;
                case CubeMapFace.PositiveY:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_Y;
                case CubeMapFace.NegativeY:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_NEGATIVE_Y;
                case CubeMapFace.PositiveZ:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_Z;
                case CubeMapFace.NegativeZ:
                    return WebGLTextureTarget.TEXTURE_CUBE_MAP_NEGATIVE_Z;

                default:
                    throw new ArgumentException();
            }
        }

        internal void PlatformConstructTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            _glTarget = WebGLTextureTarget.TEXTURE_CUBE_MAP;

            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                _glTexture = GL.CreateTexture();
                GL.CheckGLError();

                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
                GL.CheckGLError();
                GL.BindTexture(WebGLTextureTarget.TEXTURE_CUBE_MAP, _glTexture);
                GL.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy,
                    out _glInternalFormat,
                    out _glFormat,
                    out _glType,
                    out _glIsCompressedTexture
                    );


                for (int i = 0; i < 6; i++)
                {
                    WebGLTextureTarget target = ConcreteTextureCube.GetGLCubeFace((CubeMapFace)i);

                    int s = size;
                    int level = 0;
                    while (true)
                    {
                        if (_glIsCompressedTexture)
                        {
                            Rectangle bounds = new Rectangle(0, 0, s, s);
                            int dataSize = GetCompressedDataByteSize(format.GetSize(), bounds, ref bounds, out Rectangle checkedRect);
                            byte[] data = new byte[dataSize]; // WebGL CompressedTexImage2D requires data.
                            GL.CompressedTexImage2D(target, level, _glInternalFormat, checkedRect.Width, checkedRect.Height, data);
                            GL.CheckGLError();
                        }
                        else
                        {
                            GL.TexImage2D(target, level, _glInternalFormat, s, s, _glFormat, _glType);
                            GL.CheckGLError();
                        }

                        if ((s == 1) || !mipMap)
                            break;
                        if (s > 1)
                            s = s / 2;
                        ++level;
                    }
                }
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
