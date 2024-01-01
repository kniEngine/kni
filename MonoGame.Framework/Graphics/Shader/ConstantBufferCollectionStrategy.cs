// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConstantBufferCollectionStrategy
    {
        public ConstantBufferCollectionStrategy(int capacity)
        {
        }

        public abstract ConstantBuffer this[int index]
        {
            get;
            set;
        }

        internal abstract void Clear();


        internal T ToConcrete<T>() where T : ConstantBufferCollectionStrategy
        {
            return (T)this;
        }
    }
}
