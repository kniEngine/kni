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

            GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

            IntPtr srcPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            GL.CheckGLError();
            srcPtr = srcPtr + offsetInBytes;

            try
            {
                if (typeof(T) == typeof(byte) && vertexStride == 1)
                {
                    byte[] dataBuffer = data as byte[];
                    Marshal.Copy(srcPtr, dataBuffer, startIndex * vertexStride, elementCount * vertexStride);
                }
                else
                {
                    byte[] tmpBuffer = new byte[elementCount * vertexStride];
                    Marshal.Copy(srcPtr, tmpBuffer, 0, tmpBuffer.Length);

                    GCHandle tmpHandle = GCHandle.Alloc(tmpBuffer, GCHandleType.Pinned);
                    try
                    {
                        IntPtr tmpPtr = tmpHandle.AddrOfPinnedObject();
                        for (int i = 0; i < elementCount; i++)
                        {
                            data[startIndex + i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
                            tmpPtr = tmpPtr + vertexStride;
                        }
                    }
                    finally
                    {
                        tmpHandle.Free();
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
