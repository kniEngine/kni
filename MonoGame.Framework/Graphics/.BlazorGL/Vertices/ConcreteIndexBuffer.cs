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
    public class ConcreteIndexBuffer : IndexBufferStrategy
    {
        internal bool _isDynamic;

        private WebGLBuffer _ibo;

        internal WebGLBuffer GLIndexBuffer { get { return _ibo; } }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            this._isDynamic = isDynamic;

            PlatformConstructIndexBuffer();
        }

        private void PlatformConstructIndexBuffer()
        {
            var GL = this.GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            Debug.Assert(_ibo == null);

            int sizeInBytes = this.IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            _ibo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, _ibo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          sizeInBytes, _isDynamic ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            Debug.Assert(GLIndexBuffer != null);

            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementSizeInByte * elementCount;

            int bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, GLIndexBuffer);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                    GL.BufferData(
                        WebGLBufferType.ELEMENT_ARRAY,
                        bufferSize,
                        _isDynamic ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            }

            GL.BufferSubData<T>(WebGLBufferType.ELEMENT_ARRAY, offsetInBytes, data, elementCount);
            GraphicsExtensions.CheckGLError();
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            Debug.Assert(GLIndexBuffer != null);
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
                if (_ibo != null)
                {
                    _ibo.Dispose();
                    _ibo = null;
                }
            }

            base.Dispose(disposing);
        }
    }

}
