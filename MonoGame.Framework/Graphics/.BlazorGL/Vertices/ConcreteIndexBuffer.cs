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

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            this._isDynamic = isDynamic;
        }
        

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
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
