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
    public class ConcreteDynamicIndexBuffer : ConcreteIndexBuffer
    {
        internal ConcreteDynamicIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic:true)
        {
            PlatformConstructDynamicIndexBuffer();
        }
        
        private void PlatformConstructDynamicIndexBuffer()
        {
            base.PlatformConstructIndexBuffer();
        }

    }

}
