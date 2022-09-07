// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        private IWebGLRenderingContext GL { get { return this.GraphicsDevice._glContext; } }

        internal WebGLBuffer vbo { get; private set; }

        private void PlatformConstruct()
        {

            GenerateIfRequired();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        void GenerateIfRequired()
        {
            if (vbo == null)
            {
                vbo = GL.CreateBuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(WebGLBufferType.ARRAY,
                              (VertexDeclaration.VertexStride * VertexCount),
                              (_isDynamic) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
            where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(
            int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
            where T : struct
        {
            GenerateIfRequired();

            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GraphicsExtensions.CheckGLError();

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(
                    WebGLBufferType.ARRAY,
                    bufferSize,
                    _isDynamic ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
                GraphicsExtensions.CheckGLError();
            }

            if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
            {
                // there are no gaps so we can copy in one go
                GL.BufferSubData(WebGLBufferType.ARRAY, offsetInBytes, data, elementCount);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                // else we must copy each element separately
                throw new NotImplementedException();
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vbo.Dispose();
                vbo = null;
            }

            base.Dispose(disposing);
        }
    }
}
