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
    public class ConcreteDynamicVertexBuffer : ConcreteVertexBuffer, IDynamicVertexBufferStrategy
    {
        private bool _isContentLost;

        internal ConcreteDynamicVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage, isDynamic:true)
        {
            PlatformConstructDynamicVertexBuffer(contextStrategy);
        }

        private void PlatformConstructDynamicVertexBuffer(GraphicsContextStrategy contextStrategy)
        {
            base.PlatformConstructVertexBuffer(contextStrategy);
        }


        #region IDynamicVertexBufferStrategy
        public bool IsContentLost
        {
            get
            {
                throw new NotImplementedException("IsContentLost");
                return _isContentLost;
            }
        }
        #endregion IDynamicVertexBufferStrategy

    }

}
