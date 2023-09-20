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
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : VertexBufferStrategy
    {
        internal bool _isDynamic;

        //private uint _vao;
        private int _vbo;

        internal int GLVertexBuffer { get { return _vbo; } }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, bufferUsage)
        {
            this._isDynamic = isDynamic;

            PlatformConstructVertexBuffer();
        }

        private void PlatformConstructVertexBuffer()
        {
            Threading.EnsureUIThread();

            Debug.Assert(_vbo == 0);

            //this._vao = GLExt.Oes.GenVertexArray();
            //GLExt.Oes.BindVertexArray(this._vao);
            this._vbo = GL.GenBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(this.VertexDeclaration.VertexStride * this.VertexCount), IntPtr.Zero,
                         _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
            GraphicsExtensions.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLVertexBuffer != 0);

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
                    _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
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

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Vertex buffers are write-only on OpenGL ES platforms");
#else
            GetData_SDL(offsetInBytes, data, startIndex, elementCount, vertexStride);
#endif
        }

#if !GLES
        private void GetData_SDL<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLVertexBuffer != 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            // Pointer to the start of data in the vertex buffer
            IntPtr ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            GraphicsExtensions.CheckGLError();

            ptr = (IntPtr)(ptr.ToInt64() + offsetInBytes);

            if (typeof(T) == typeof(byte) && vertexStride == 1)
            {
                // If data is already a byte[] and stride is 1 we can skip the temporary buffer
                byte[] buffer = data as byte[];
                Marshal.Copy(ptr, buffer, startIndex * vertexStride, elementCount * vertexStride);
            }
            else
            {
                // Temporary buffer to store the copied section of data
                byte[] tmp = new byte[elementCount * vertexStride];
                // Copy from the vertex buffer to the temporary buffer
                Marshal.Copy(ptr, tmp, 0, tmp.Length);

                // Copy from the temporary buffer to the destination array
                GCHandle tmpHandle = GCHandle.Alloc(tmp, GCHandleType.Pinned);
                try
                {
                    IntPtr tmpPtr = tmpHandle.AddrOfPinnedObject();
                    for (int i = 0; i < elementCount; i++)
                    {
                        data[startIndex + i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
                        tmpPtr = (IntPtr)(tmpPtr.ToInt64() + vertexStride);
                    }
                }
                finally
                {
                    tmpHandle.Free();
                }
            }

            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GraphicsExtensions.CheckGLError();
        }

#endif

        internal override void PlatformGraphicsDeviceResetting()
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
                    GL.DeleteBuffer(_vbo);
                    GraphicsExtensions.CheckGLError();
                    _vbo = 0;
                }
            }

            base.Dispose(disposing);
        }
    }

}
