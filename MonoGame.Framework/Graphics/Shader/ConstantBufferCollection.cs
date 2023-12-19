// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    internal sealed class ConstantBufferCollection
    {
        private ConstantBufferCollectionStrategy _strategy;

        internal ConstantBufferCollectionStrategy Strategy { get { return _strategy; } }


        public ConstantBufferCollection(GraphicsContextStrategy contextStrategy, int capacity)
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
