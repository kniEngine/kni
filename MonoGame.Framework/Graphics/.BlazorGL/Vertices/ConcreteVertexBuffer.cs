// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : VertexBufferStrategy
    {
        internal bool _isDynamic;

        internal WebGLBuffer _vbo;

        internal WebGLBuffer GLVertexBuffer { get { return _vbo; } }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            Debug.Assert(isDynamic == true);
            this._isDynamic = isDynamic;
        }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            this._isDynamic = false;

            PlatformConstructVertexBuffer();
        }

        internal void PlatformConstructVertexBuffer()
        {
            Debug.Assert(_vbo == null);

            var GL = this.GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _vbo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, _vbo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            GL.BufferData(WebGLBufferType.ARRAY,
                          (this.VertexDeclaration.VertexStride * this.VertexCount),
                          (_isDynamic) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            Debug.Assert(GLVertexBuffer != null);

            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            GL.BindBuffer(WebGLBufferType.ARRAY, GLVertexBuffer);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._vertexBuffersDirty = true;

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    WebGLBufferType.ARRAY,
                    bufferSize,
                    _isDynamic ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
                GraphicsExtensions.CheckGLError();
            }

            if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
            {
                // there are no gaps so we can copy in one go
                GL.BufferSubData<T>(WebGLBufferType.ARRAY, offsetInBytes, data, elementCount);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                // else we must copy each element separately
                throw new NotImplementedException();
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            Debug.Assert(GLVertexBuffer != null);
            throw new NotImplementedException();
        }

        internal override void PlatformGraphicsDeviceResetting()
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
