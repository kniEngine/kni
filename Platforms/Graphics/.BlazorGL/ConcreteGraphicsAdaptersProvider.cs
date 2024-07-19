// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

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
            List<GraphicsAdapter> adapterList = CreateAdapterList();
            _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
        }

        private List<GraphicsAdapter> CreateAdapterList()
        {
            List<GraphicsAdapter> adapterList = new List<GraphicsAdapter>(1);

            ConcreteGraphicsAdapter adapterStrategy = new ConcreteGraphicsAdapter();
            GraphicsAdapter adapter = base.CreateGraphicsAdapter(adapterStrategy);

            adapterList.Add(adapter);

            // The first adapter is considered the default.
            ((IPlatformGraphicsAdapter)adapterList[0]).Strategy.Platform_IsDefaultAdapter = true;

            return adapterList;
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
