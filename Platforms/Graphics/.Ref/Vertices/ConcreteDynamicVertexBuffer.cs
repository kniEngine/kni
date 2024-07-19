// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteDynamicVertexBuffer : ConcreteVertexBuffer, IDynamicVertexBufferStrategy
    {

        internal ConcreteDynamicVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage, isDynamic:true)
        {
            throw new PlatformNotSupportedException();
        }


        #region IDynamicVertexBufferStrategy
        public bool IsContentLost
        {
            get { throw new PlatformNotSupportedException(); }
        }
        #endregion IDynamicVertexBufferStrategy

    }

}
