// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Android.Views;
using Android.Runtime;


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

            _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
            return;
        }

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { return _adapters; }
        }

        internal override GraphicsAdapter Platform_DefaultAdapter
        {
            get { return _adapters[0]; }
        }
        
    }
}
