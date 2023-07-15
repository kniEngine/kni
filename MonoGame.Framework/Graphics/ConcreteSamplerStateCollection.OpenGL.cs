// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteSamplerStateCollection : SamplerStateCollectionStrategy
    {

        internal ConcreteSamplerStateCollection(GraphicsDevice device, GraphicsContext context, int capacity)
            : base(device, context, capacity)
        {
        }


    }
}
