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
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;


        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format,
                                   bool isRenderTarget)
            : this(contextStrategy, width, height, depth, mipMap, format)
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            Threading.EnsureUIThread();

            {
                int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    GL.BindTexture(_glTarget, _glTexture);
                    GraphicsExtensions.CheckGLError();

                    GL.TexSubImage3D(_glTarget, level, left, top, front, width, height, depth, _glFormat, _glType, dataPtr);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
            throw new NotImplementedException();
        }
        #endregion #region ITexture3DStrategy


        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            _glTarget = TextureTarget.Texture3D;

            Threading.EnsureUIThread();
            {
                _glTexture = GL.GenTexture();
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(_glTarget, _glTexture);
                GraphicsExtensions.CheckGLError();

                ConcreteTexture.ToGLSurfaceFormat(format, contextStrategy.Context.DeviceStrategy, out _glInternalFormat, out _glFormat, out _glType);

                GL.TexImage3D(_glTarget, 0, _glInternalFormat, width, height, depth, 0, _glFormat, _glType, IntPtr.Zero);
                GraphicsExtensions.CheckGLError();
            }

            if (mipMap)
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
        }


    }
}
