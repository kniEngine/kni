// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteVertexBufferGL : VertexBufferStrategy
    {
        private readonly BufferUsageHint _usageHint;
        //private uint _vao;
        private int _vbo;

        internal int GLVertexBuffer { get { return _vbo; } }

        internal ConcreteVertexBufferGL(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            Debug.Assert(isDynamic == true);
            _usageHint = BufferUsageHint.DynamicDraw;
        }

        internal ConcreteVertexBufferGL(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            _usageHint = BufferUsageHint.StaticDraw;

            PlatformConstructVertexBuffer();
        }

        internal void PlatformConstructVertexBuffer()
        {
            Threading.EnsureUIThread();

            Debug.Assert(_vbo == 0);

            var GL = OGL.Current;

            //this._vao = GLExt.Oes.GenVertexArray();
            //GLExt.Oes.BindVertexArray(this._vao);
            this._vbo = GL.GenBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(this.VertexDeclaration.VertexStride * this.VertexCount), IntPtr.Zero,
                         _usageHint);
            GraphicsExtensions.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLVertexBuffer != 0);

            var GL = OGL.Current;

            GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    BufferTarget.ArrayBuffer,
                    (IntPtr)bufferSize,
                    IntPtr.Zero,
                    _usageHint);
                GraphicsExtensions.CheckGLError();
            }

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            if (elementSizeInByte == vertexStride || elementSizeInByte % vertexStride == 0)
            {
                // there are no gaps so we can copy in one go
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)(elementSizeInBytes * elementCount), dataPtr);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }
            }
            else
            {
                // else we must copy each element separately
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    int dstOffset = offsetInBytes;
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    for (int i = 0; i < elementCount; i++)
                    {
                        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)dstOffset, (IntPtr)elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();

                        dstOffset += vertexStride;
                        dataPtr = (IntPtr)(dataPtr.ToInt64() + elementSizeInByte);
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        internal override void PlatformGraphicsContextLost()
        {
            _vbo = 0;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_vbo != 0)
            {
                if (GraphicsDevice != null && !GraphicsDevice.IsDisposed)
                {
                    var GL = OGL.Current;

                    GL.DeleteBuffer(_vbo);
                    GraphicsExtensions.CheckGLError();
                    _vbo = 0;
                }
            }

            base.Dispose(disposing);
        }
    }

}
