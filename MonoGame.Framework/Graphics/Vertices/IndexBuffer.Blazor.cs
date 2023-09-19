// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        internal WebGLBuffer ibo { get; private set; }

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {

            GenerateIfRequired();
        }

        void GenerateIfRequired()
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            if (ibo != null)
                return;

            var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            ibo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          sizeInBytes, _isDynamic ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            var GL = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            GenerateIfRequired();

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            var sizeInBytes = elementSizeInByte * elementCount;

            var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
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

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ibo.Dispose();
                ibo = null;
            }

            base.Dispose(disposing);
        }
    }
}
