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
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteIndexBuffer : ConcreteIndexBufferGL
    {
        public ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic)
        {
        }

        public ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
        }

        public unsafe override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            bool isSharedContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().BindSharedContext();
            try
            {
                Debug.Assert(GLIndexBuffer != 0);

                // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
                // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
                if (this.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                    throw new NotSupportedException("Index buffers are write-only on OpenGL ES platforms");

                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
                GL.CheckGLError();
                if (!isSharedContext)
                    ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;
                
                int elementSizeInBytes = sizeof(T);

                fixed (T* pData = &data[0])
                {
                    IntPtr dataPtr = (IntPtr)pData;
                    dataPtr = dataPtr + startIndex * elementSizeInBytes;

                    // there are no gaps so we can copy in one go
                    IntPtr srcPtr = GL.MapBufferRange(
                        BufferTarget.ElementArrayBuffer,
                        (IntPtr)offsetInBytes,
                        (IntPtr)(elementSizeInBytes * elementCount),
                        BufferRangeAccess.Read);
                    if (srcPtr == IntPtr.Zero)
                        throw new InvalidOperationException("glMapBufferRange failed.");

                    MemCopyHelper.MemoryCopy(
                        srcPtr,
                        dataPtr,
                        elementSizeInBytes * elementCount);

                    GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
                    GL.CheckGLError();
                }
            }
            finally
            {
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindSharedContext();
            }
        }
    }

}
