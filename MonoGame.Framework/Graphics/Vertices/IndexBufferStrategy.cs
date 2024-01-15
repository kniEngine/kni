// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformIndexBuffer
    {
        IndexBufferStrategy Strategy { get; }
    }

    public abstract class IndexBufferStrategy : GraphicsResourceStrategy
    {
        internal readonly GraphicsContextStrategy _contextStrategy;

        private IndexElementSize _indexElementSize;
        private int _indexCount;
        private BufferUsage _bufferUsage;

        private readonly int _elementSizeInBytes;

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

        public int ElementSizeInBytes
        {
            get { return _elementSizeInBytes; }
        }

        public T ToConcrete<T>() where T : IndexBufferStrategy
        {
            return (T)this;
        }

        protected IndexBufferStrategy(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy)
        {
            _contextStrategy = contextStrategy;

            this._indexElementSize = indexElementSize;	
            this._indexCount = indexCount;
            this._bufferUsage = usage;

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits:   this._elementSizeInBytes = sizeof(Int16); break;
                case IndexElementSize.ThirtyTwoBits: this._elementSizeInBytes = sizeof(Int32); break;
                default: throw new InvalidOperationException();
            }
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
