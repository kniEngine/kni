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

                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();

                int startBytes = startIndex * elementSizeInByte;
                if (startIndex != 0 && !_glIsCompressedTexture)
                    throw new NotImplementedException("startIndex");

                ((IPlatformTextureCollection)base.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
                GL.CheckGLError();
                GL.BindTexture(WebGLTextureTarget.TEXTURE_CUBE_MAP, _glTexture);
                GL.CheckGLError();

                WebGLTextureTarget target = ConcreteTextureCube.GetGLCubeFace(face);
                if (_glIsCompressedTexture)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    GL.TexSubImage2D(target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, _glFormat, _glType, data);
                    GL.CheckGLError();
                }
            }
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            // round x and y down to next multiple of four; width and height up to next multiple of four
            int roundedWidth = (rect.Width + 3) & ~0x3;
            int roundedHeight = (rect.Height + 3) & ~0x3;
            checkedRect = new Rectangle(rect.X & ~0x3, rect.Y & ~0x3,
#if OPENGL
                    // OpenGL only: The last two mip levels require the width and height to be
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy
                    // a 4x4 block.
                    (rect.Width < 4 && textureBounds.Width < 4) ? textureBounds.Width : roundedWidth,
                    (rect.Height < 4 && textureBounds.Height < 4) ? textureBounds.Height : roundedHeight);
#else
                                        roundedWidth, roundedHeight);
#endif
            return (roundedWidth * roundedHeight * fSize / 16);
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

                    if (_glIsCompressedTexture)
                    {
                         throw new NotImplementedException();
                    }
                    else
                    {
                        GL.TexImage2D(target, 0, _glInternalFormat, size, size, _glFormat, _glType);
                        GL.CheckGLError();
                    }
                }

                if (mipMap)
                {
                    GL.GenerateMipmap(WebGLTextureTarget.TEXTURE_CUBE_MAP);
                    GL.CheckGLError();
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
