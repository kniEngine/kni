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
    public class ConcreteIndexBuffer : IndexBufferStrategy
    {
        internal bool _isDynamic;

        private int _ibo;

        internal int GLIndexBuffer { get { return _ibo; } }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            this._isDynamic = isDynamic;

            PlatformConstructIndexBuffer();
        }

        private void PlatformConstructIndexBuffer()
        {
            Threading.EnsureUIThread();

            Debug.Assert(_ibo == 0);

            int sizeInBytes = this.IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            _ibo = GL.GenBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
            GraphicsExtensions.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLIndexBuffer != 0);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementSizeInByte * elementCount;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                int bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
                GraphicsExtensions.CheckGLError();
                this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        BufferTarget.ElementArrayBuffer,
                        (IntPtr)bufferSize,
                        IntPtr.Zero,
                        _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
                    GraphicsExtensions.CheckGLError();
                }

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Index buffers are write-only on OpenGL ES platforms");
#else
            GetData_SDL(offsetInBytes, data, startIndex, elementCount);
#endif
        }


#if !GLES

        private void GetData_SDL<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLIndexBuffer != 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            IntPtr ptr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly);
            // Pointer to the start of data to read in the index buffer
            ptr = new IntPtr(ptr.ToInt64() + offsetInBytes);
            if (typeof(T) == typeof(byte))
            {
                byte[] buffer = data as byte[];
                // If data is already a byte[] we can skip the temporary buffer
                // Copy from the index buffer to the destination array
                Marshal.Copy(ptr, buffer, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            else
            {
                // Temporary buffer to store the copied section of data
                byte[] buffer = new byte[elementCount * elementSizeInByte];
                // Copy from the index buffer to the temporary buffer
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                // Copy from the temporary buffer to the destination array
                Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            GraphicsExtensions.CheckGLError();
        }
#endif

        internal override void PlatformGraphicsDeviceResetting()
        {
            _ibo = 0;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_ibo != 0)
            {
                if (GraphicsDevice != null && !GraphicsDevice.IsDisposed)
                {
                    GL.DeleteBuffer(_ibo);
                    GraphicsExtensions.CheckGLError();
                    _ibo = 0;
                }
            }

            base.Dispose(disposing);
        }
    }

}
