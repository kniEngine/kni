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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : VertexBufferStrategy
    {
        internal bool _isDynamic;

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, bufferUsage)
        {
            this._isDynamic = isDynamic;
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
        }

        internal override void PlatformGraphicsDeviceResetting()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }

}
