// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConstantBufferCollection
        : IPlatformConstantBufferCollection
    {
        private ConstantBufferCollectionStrategy _strategy;

        ConstantBufferCollectionStrategy IPlatformConstantBufferCollection.Strategy { get { return _strategy; } }


        internal ConstantBufferCollection(GraphicsContextStrategy contextStrategy, int capacity)
        {
            _strategy = contextStrategy.CreateConstantBufferCollectionStrategy(capacity);
        }


        public ConstantBuffer this[int index]
        {
            get { return _strategy[index]; }
            set { _strategy[index] = value; }
        }

        internal void Clear()
        {
            _strategy.Clear();
        }

    }
}
