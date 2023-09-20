// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class IndexBufferStrategy : GraphicsResourceStrategy
    {
        internal readonly GraphicsContextStrategy _contextStrategy;

        private IndexElementSize _indexElementSize;
        private int _indexCount;
        private BufferUsage _bufferUsage;

        public IndexElementSize IndexElementSize
        {
            get { return _indexElementSize; }
        }
        public int IndexCount
        {
            get { return _indexCount; }
        }
        public BufferUsage BufferUsage
        {
            get { return _bufferUsage; }
        }

        internal T ToConcrete<T>() where T : IndexBufferStrategy
        {
            return (T)this;
        }

        internal IndexBufferStrategy(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy)
        {
			this._indexElementSize = indexElementSize;	
            this._indexCount = indexCount;
            this._bufferUsage = usage;

        }

        public abstract void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct;
        public abstract void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct;


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
