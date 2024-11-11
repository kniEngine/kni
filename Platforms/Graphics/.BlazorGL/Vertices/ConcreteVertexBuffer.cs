// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : VertexBufferStrategy
    {
        private readonly WebGLBufferUsageHint _usageHint;
        internal WebGLBuffer _vbo;

        internal WebGLBuffer GLVertexBuffer { get { return _vbo; } }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            Debug.Assert(isDynamic == true);
            _usageHint = WebGLBufferUsageHint.DYNAMIC_DRAW;
        }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            _usageHint = WebGLBufferUsageHint.STATIC_DRAW;

            PlatformConstructVertexBuffer(contextStrategy);
        }

        internal void PlatformConstructVertexBuffer(GraphicsContextStrategy contextStrategy)
        {
            {
                Debug.Assert(_vbo == null);

                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                _vbo = GL.CreateBuffer();
                GL.CheckGLError();
                GL.BindBuffer(WebGLBufferType.ARRAY, _vbo);
                GL.CheckGLError();
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

                GL.BufferData(WebGLBufferType.ARRAY,
                              (this.VertexDeclaration.VertexStride * this.VertexCount),
                              _usageHint);
                GL.CheckGLError();
            }
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            Debug.Assert(GLVertexBuffer != null);

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            GL.BindBuffer(WebGLBufferType.ARRAY, GLVertexBuffer);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    WebGLBufferType.ARRAY,
                    bufferSize,
                    _usageHint);
                GL.CheckGLError();
            }

            if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
            {
                // there are no gaps so we can copy in one go
                GL.BufferSubData<T>(WebGLBufferType.ARRAY, offsetInBytes, data, startIndex, elementCount);
                GL.CheckGLError();
            }
            else
            {
                // else we must copy each element separately
                int dstOffset = offsetInBytes;

                for (int i = 0; i < elementCount; i++)
                {
                    GL.BufferSubData<T>(WebGLBufferType.ARRAY, dstOffset, data, startIndex+i, 1);
                    GL.CheckGLError();

                    dstOffset += vertexStride;
                }
            }
        }

        public unsafe override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            Debug.Assert(GLVertexBuffer != null);

            // IWebGL2RenderingContext is required.
            if (this.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("GetData() on BlazorGL require HiDef profile or higher.");

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            GL.BindBuffer(WebGLBufferType.ARRAY, GLVertexBuffer);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._vertexBuffersDirty = true;

            int elementSizeInByte = sizeof(T);

            if (elementSizeInByte == vertexStride || elementSizeInByte % vertexStride == 0)
            {
                ((IWebGL2RenderingContext)GL).GetBufferSubData<T>(WebGLBufferType.ARRAY, 
                    offsetInBytes, data, startIndex, elementCount);
            }
            else
            {
                for (int i = 0; i < elementCount; i++)
                {
                    ((IWebGL2RenderingContext)GL).GetBufferSubData<T>(WebGLBufferType.ARRAY,
                         offsetInBytes + (i*vertexStride), data, startIndex+i, 1);
                }
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            throw new NotImplementedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_vbo != null)
                {
                    _vbo.Dispose();
                    _vbo = null;
                }
            }

            base.Dispose(disposing);
        }
    }

}
