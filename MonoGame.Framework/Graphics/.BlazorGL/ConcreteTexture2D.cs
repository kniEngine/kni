// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture2D : ConcreteTexture, ITexture2DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _arraySize;

        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;
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
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            var startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0 && !_glIsCompressedTexture)
                throw new NotImplementedException("startIndex");

            System.Diagnostics.Debug.Assert(_glTexture == null);
            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GraphicsExtensions.CheckGLError();

            GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(this.Format.GetSize(), 8));
            GraphicsExtensions.CheckGLError();

            if (_glIsCompressedTexture)
            {
                GL.CompressedTexImage2D(
                        WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, data, startIndex, elementCount);
            }
            else
            {
                GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, level, _glInternalFormat, w, h, _glFormat, _glType, data);
            }
            GraphicsExtensions.CheckGLError();

            //GL.Finish();
            //GraphicsExtensions.CheckGLError();
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();

            var startBytes = startIndex * elementSizeInByte;
            if (startIndex != 0)
                throw new NotImplementedException("startIndex");

            System.Diagnostics.Debug.Assert(_glTexture == null);
            GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, _glTexture);
            GraphicsExtensions.CheckGLError();

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
            GraphicsExtensions.CheckGLError();

            //GL.Finish();
            //GraphicsExtensions.CheckGLError();
        }

        public void GetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }
        #endregion #region ITexture2DStrategy

    }
}
