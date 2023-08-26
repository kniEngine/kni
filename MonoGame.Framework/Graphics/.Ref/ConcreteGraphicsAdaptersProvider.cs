// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdaptersProvider : GraphicsAdaptersProviderStrategy
    {

        public ConcreteGraphicsAdaptersProvider()
        {
            throw new PlatformNotSupportedException();
            return;
        }

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { throw new PlatformNotSupportedException(); }
        }

        internal override GraphicsAdapter Platform_DefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }
        
    }
}
