// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformConstantBufferCollection
    {
        ConstantBufferCollectionStrategy Strategy { get; }
    }

    public abstract class ConstantBufferCollectionStrategy
    {
        private readonly ConstantBuffer[] _buffers;

        public int Length { get { return _buffers.Length; } }


        protected ConstantBufferCollectionStrategy(int capacity)
        {
            _buffers = new ConstantBuffer[capacity];
        }

        public virtual ConstantBuffer this[int index]
        {
            get { return _buffers[index]; }
            set { _buffers[index] = value; }
        }

        public abstract void Clear();


        public T ToConcrete<T>() where T : ConstantBufferCollectionStrategy
        {
            return (T)this;
        }
    }
}
