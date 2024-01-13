// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
        private ReadOnlyCollection<GraphicsAdapter> _adapters;

        public ConcreteGraphicsAdaptersProvider()
        {
            List<GraphicsAdapter> adapterList = new List<GraphicsAdapter>(1);
            ConcreteGraphicsAdapter strategy = new ConcreteGraphicsAdapter();
            GraphicsAdapter adapter = new GraphicsAdapter(strategy);

            adapterList.Add(adapter);

            // The first adapter is considered the default.
            ((IPlatformGraphicsAdapter)adapterList[0]).Strategy.Platform_IsDefaultAdapter = true;

            _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
            return;
        }

        public override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { return _adapters; }
        }

        public override GraphicsAdapter Platform_DefaultAdapter
        {
            get { return _adapters[0]; }
        }
        
    }
}
