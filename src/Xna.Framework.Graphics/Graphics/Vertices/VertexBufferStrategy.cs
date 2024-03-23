// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformVertexBuffer
    {
        VertexBufferStrategy Strategy { get; }
    }

    public abstract class VertexBufferStrategy : GraphicsResourceStrategy
    {
        protected readonly GraphicsContextStrategy _contextStrategy;

        private VertexDeclaration _vertexDeclaration;
        private int _vertexCount;
        private BufferUsage _bufferUsage;

        public VertexDeclaration VertexDeclaration
        {
            get { return _vertexDeclaration; }
        }
        public int VertexCount
        {
            get { return _vertexCount; }
        }
        public BufferUsage BufferUsage
        {
            get { return _bufferUsage; }
        }

        public T ToConcrete<T>() where T : VertexBufferStrategy
        {
            return (T)this;
        }

        protected VertexBufferStrategy(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy)
        {
            this._contextStrategy = contextStrategy;

            this._vertexDeclaration = vertexDeclaration;
            this._vertexCount = vertexCount;
            this._bufferUsage = usage;
        }

        public abstract void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct;
        public abstract void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
