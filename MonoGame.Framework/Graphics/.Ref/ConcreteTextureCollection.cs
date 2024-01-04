// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {

        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
            throw new PlatformNotSupportedException();
        }


        public override void Clear()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApply()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
