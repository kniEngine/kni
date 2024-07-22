// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteDynamicIndexBuffer : ConcreteIndexBuffer, IDynamicIndexBufferStrategy
    {

        internal ConcreteDynamicIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic:true)
        {
            throw new PlatformNotSupportedException();
        }


        #region IDynamicIndexBufferStrategy
        public bool IsContentLost
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion IDynamicIndexBufferStrategy

    }

}
