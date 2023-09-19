// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        internal int _ibo;

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {
            Threading.EnsureUIThread();
            GenerateIfRequired();
        }

        /// <summary>
        /// If the IBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (_ibo != 0)
                return;

            int sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            _ibo = GL.GenBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Index buffers are write-only on OpenGL ES platforms");
#else
            Threading.EnsureUIThread();

            GetBufferData(offsetInBytes, data, startIndex, elementCount);
#endif
        }

#if !GLES

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
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

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            Threading.EnsureUIThread();

            GenerateIfRequired();

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementSizeInByte * elementCount;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                int bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
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

        private void PlatformGraphicsDeviceResetting()
        {
            _ibo = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    if (!GraphicsDevice.IsDisposed)
                    {
                        GL.DeleteBuffer(_ibo);
                        GraphicsExtensions.CheckGLError();
                    }
                }
            }
            base.Dispose(disposing);
        }

    }
}
