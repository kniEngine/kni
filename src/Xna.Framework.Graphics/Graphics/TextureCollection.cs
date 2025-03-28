﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class TextureCollection
        : IPlatformTextureCollection
    {
        private TextureCollectionStrategy _strategy;

        TextureCollectionStrategy IPlatformTextureCollection.Strategy { get { return _strategy; } }


        internal TextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
        {
            _strategy = contextStrategy.CreateTextureCollectionStrategy(capacity);

        }

        public Texture this[int index]
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
