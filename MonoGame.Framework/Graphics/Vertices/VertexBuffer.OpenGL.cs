// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        //internal uint _vao;
        internal int _vbo;

        private void PlatformConstructVertexBuffer()
        {
            Threading.EnsureUIThread();
            GenerateIfRequired();
        }

        /// <summary>
        /// If the VBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (_vbo == 0)
            {
                //this._vao = GLExt.Oes.GenVertexArray();
                //GLExt.Oes.BindVertexArray(this._vao);
                this._vbo = GL.GenBuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
                GraphicsExtensions.CheckGLError();
                this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(VertexDeclaration.VertexStride * VertexCount), IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Vertex buffers are write-only on OpenGL ES platforms");
#else
            Threading.EnsureUIThread();

            GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride);
#endif
        }

#if !GLES

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
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

        private void PlatformSetData<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            Threading.EnsureUIThread();

            GenerateIfRequired();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
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

        private void PlatformGraphicsDeviceResetting()
        {
            _vbo = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    if (!GraphicsDevice.IsDisposed)
                    {
                        GL.DeleteBuffer(_vbo);
                        GraphicsExtensions.CheckGLError();
                    }
                }
            }
            base.Dispose(disposing);
        }

    }
}
