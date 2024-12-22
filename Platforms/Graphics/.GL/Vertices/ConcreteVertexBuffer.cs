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
using Microsoft.Xna.Platform.Graphics.Utilities;
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

            PlatformConstructVertexBuffer(contextStrategy);
        }

        internal void PlatformConstructVertexBuffer(GraphicsContextStrategy contextStrategy)
        {
            bool isSharedContext = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindSharedContext();
            try
            {
                Debug.Assert(_vbo == 0);

                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                //this._vao = GLExt.Oes.GenVertexArray();
                //GLExt.Oes.BindVertexArray(this._vao);
                this._vbo = GL.GenBuffer();
                GL.CheckGLError();
                GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
                GL.CheckGLError();
                if (!isSharedContext)
                    contextStrategy._vertexBuffersDirty = true;

                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(this.VertexDeclaration.VertexStride * this.VertexCount), IntPtr.Zero,
                             _usageHint);
                GL.CheckGLError();
            }
            finally
            {
                contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindSharedContext();
            }
        }

        public unsafe override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            bool isSharedContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().BindSharedContext();
            try
            {
                Debug.Assert(GLVertexBuffer != 0);

                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

                GL.BindBuffer(BufferTarget.ArrayBuffer, GLVertexBuffer);
                GL.CheckGLError();
                if (!isSharedContext)
                    ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        BufferTarget.ArrayBuffer,
                        (IntPtr)bufferSize,
                        IntPtr.Zero,
                        _usageHint);
                    GL.CheckGLError();
                }

                fixed (T* pData = &data[0])
                {
                    IntPtr dataPtr = (IntPtr)pData;
                    dataPtr = dataPtr + startIndex * elementSizeInBytes;

                    if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
                    {
                        // there are no gaps so we can copy in one go
                        GL.BufferSubData(BufferTarget.ArrayBuffer, 
                            (IntPtr)offsetInBytes,
                            (IntPtr)(elementSizeInBytes * elementCount),
                            dataPtr);
                        GL.CheckGLError();
                    }
                    else
                    {
                        // else we must copy each element separately
                        for (int i = 0; i < elementCount; i++)
                        {
                            GL.BufferSubData(BufferTarget.ArrayBuffer,
                                (IntPtr)offsetInBytes + i*vertexStride,
                                (IntPtr)elementSizeInBytes,
                                dataPtr + i*elementSizeInBytes);
                            GL.CheckGLError();
                        }
                    }
                }
            }
            finally
            {
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindSharedContext();
            }
        }

        protected override void PlatformGraphicsContextLost()
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
                    _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindDisposeContext();
                    try
                    {
                        var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                        GL.DeleteBuffer(_vbo);
                        GL.CheckGLError();
                        _vbo = 0;
                    }
                    finally
                    {
                        _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindDisposeContext();
                    }
                }
            }

            base.Dispose(disposing);
        }
    }

}
