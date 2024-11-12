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
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : ConcreteVertexBufferGL
    {
        public ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage, isDynamic)
        {
        }

        public ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
        }

        public unsafe override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            bool isSharedContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().BindSharedContext();
            try
            {
                Debug.Assert(GLVertexBuffer != 0);

                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                int elementSizeInBytes = sizeof(T);
                int sizeInBytes = elementCount * elementSizeInBytes;

                GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
                GL.CheckGLError();
                if (!isSharedContext)
                    ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

                IntPtr srcPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
                GL.CheckGLError();
                try
                {
                    srcPtr = srcPtr + offsetInBytes;

                    fixed (T* pData = &data[0])
                    {
                        IntPtr dataPtr = (IntPtr)pData;
                        dataPtr = dataPtr + startIndex * elementSizeInBytes;

                        if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
                        {
                            MemCopyHelper.MemoryCopy(
                               srcPtr,
                               dataPtr,
                               sizeInBytes);
                        }
                        else
                        {
                            for (int i = 0; i < elementCount; i++)
                            {
                                MemCopyHelper.MemoryCopy(
                                   srcPtr + i * vertexStride,
                                   dataPtr + i * elementSizeInBytes,
                                   elementSizeInBytes);
                            }
                        }
                    }
                }
                finally
                {
                    GL.UnmapBuffer(BufferTarget.ArrayBuffer);
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
