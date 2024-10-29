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

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            Debug.Assert(GLVertexBuffer != 0);

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementCount * elementSizeInBytes;

            GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

            IntPtr srcPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            GL.CheckGLError();
            srcPtr = srcPtr + offsetInBytes;

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                dataPtr = dataPtr + startIndex * elementSizeInBytes;

                if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
                {
                     Microsoft.Xna.Platform.Utilities.MemCopyHelper.MemoryCopy(
                        srcPtr,
                        dataPtr,
                        sizeInBytes);
                }
                else
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                         Microsoft.Xna.Platform.Utilities.MemCopyHelper.MemoryCopy(
                            srcPtr + i * vertexStride,
                            dataPtr + i * elementSizeInBytes,
                            sizeInBytes);
                    }
                }
            }
            finally
            {
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                GL.CheckGLError();
            }
        }
    }

}
