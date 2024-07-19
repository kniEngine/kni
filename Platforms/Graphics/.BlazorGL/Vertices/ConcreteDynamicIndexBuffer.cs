// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteDynamicIndexBuffer : ConcreteIndexBuffer, IDynamicIndexBufferStrategy
    {
        private bool _isContentLost;

        internal ConcreteDynamicIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic:true)
        {
            PlatformConstructDynamicIndexBuffer(contextStrategy);
        }
        
        private void PlatformConstructDynamicIndexBuffer(GraphicsContextStrategy contextStrategy)
        {
            base.PlatformConstructIndexBuffer(contextStrategy);
        }


        #region IDynamicIndexBufferStrategy
        public bool IsContentLost
        {
            get
            {
                throw new NotImplementedException("IsContentLost");
                return _isContentLost;
            }
        }
        #endregion IDynamicIndexBufferStrategy

    }

}
