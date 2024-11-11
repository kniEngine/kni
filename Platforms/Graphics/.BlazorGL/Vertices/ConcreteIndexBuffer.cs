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
    public class ConcreteIndexBuffer : IndexBufferStrategy
    {
        private readonly WebGLBufferUsageHint _usageHint;
        private readonly WebGLDataType _drawElementsType;
        private WebGLBuffer _ibo;

        internal WebGLDataType DrawElementsType { get { return _drawElementsType; } }
        internal WebGLBuffer GLIndexBuffer { get { return _ibo; } }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            Debug.Assert(isDynamic == true);
            _usageHint= WebGLBufferUsageHint.DYNAMIC_DRAW;

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits:   this._drawElementsType = WebGLDataType.USHORT; break;
                case IndexElementSize.ThirtyTwoBits: this._drawElementsType = WebGLDataType.UINT; break;
                default: throw new InvalidOperationException();
            }
        }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            _usageHint = WebGLBufferUsageHint.STATIC_DRAW;

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits: this._drawElementsType = WebGLDataType.USHORT; break;
                case IndexElementSize.ThirtyTwoBits: this._drawElementsType = WebGLDataType.UINT; break;
                default: throw new InvalidOperationException();
            }

            PlatformConstructIndexBuffer(contextStrategy);
        }

        internal void PlatformConstructIndexBuffer(GraphicsContextStrategy contextStrategy)
        {
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                Debug.Assert(_ibo == null);

                int sizeInBytes = this.IndexCount * base.ElementSizeInBytes;

                _ibo = GL.CreateBuffer();
                GL.CheckGLError();
                GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, _ibo);
                GL.CheckGLError();
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

                GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                              sizeInBytes, _usageHint);
                GL.CheckGLError();
            }
        }

        public unsafe override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            {
                Debug.Assert(GLIndexBuffer != null);

                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                int elementSizeInByte = sizeof(T);
                int sizeInBytes = elementSizeInByte * elementCount;

                int bufferSize = IndexCount * base.ElementSizeInBytes;

                GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, GLIndexBuffer);
                GL.CheckGLError();
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        WebGLBufferType.ELEMENT_ARRAY,
                        bufferSize,
                        _usageHint);
                }

                GL.BufferSubData<T>(WebGLBufferType.ELEMENT_ARRAY, offsetInBytes, data, startIndex, elementCount);
                GL.CheckGLError();
            }
        }

        public unsafe override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            {
                Debug.Assert(GLIndexBuffer != null);

                // IWebGL2RenderingContext is required.
                if (this.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                    throw new NotSupportedException("GetData() on BlazorGL require HiDef profile or higher.");

                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, GLIndexBuffer);
                GL.CheckGLError();
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

                int elementSizeInByte = sizeof(T);

                ((IWebGL2RenderingContext)GL).GetBufferSubData<T>(WebGLBufferType.ELEMENT_ARRAY,
                    offsetInBytes, data, startIndex, elementCount);

                GL.CheckGLError();
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
