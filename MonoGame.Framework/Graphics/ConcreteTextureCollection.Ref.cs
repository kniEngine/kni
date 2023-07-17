// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {

        internal ConcreteTextureCollection(GraphicsDevice device, GraphicsContext context, int capacity)
            : base(device, context, capacity)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void Clear()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApply()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
