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
        protected ConstantBufferCollectionStrategy(int capacity)
        {
        }

        public abstract ConstantBuffer this[int index]
        {
            get;
            set;
        }

        public abstract void Clear();


        public T ToConcrete<T>() where T : ConstantBufferCollectionStrategy
        {
            return (T)this;
        }
    }
}
