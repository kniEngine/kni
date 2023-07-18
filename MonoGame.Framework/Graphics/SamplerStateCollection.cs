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
	{
        private SamplerStateCollectionStrategy _strategy;


        internal SamplerStateCollectionStrategy Strategy { get { return _strategy; } }


        internal SamplerStateCollection(GraphicsDevice device, GraphicsContext context, int capacity)
        {
            _strategy = new ConcreteSamplerStateCollection(device, context, capacity);

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

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            _strategy.Dirty();
        }
    }
}
