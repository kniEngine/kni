// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
        : IPlatformSamplerStateCollection
    {
        private SamplerStateCollectionStrategy _strategy;


        SamplerStateCollectionStrategy IPlatformSamplerStateCollection.Strategy { get { return _strategy; } }


        internal SamplerStateCollection(GraphicsContextStrategy contextStrategy, int capacity)
        {
            _strategy = contextStrategy.CreateSamplerStateCollectionStrategy(capacity);
            
            Clear();
        }
        
        public SamplerState this[int index] 
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
