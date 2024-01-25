// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;


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
            // NOTE: An adapter is a monitor+device combination, so we expect
            // at lease one adapter per connected monitor.

            using (DXGI.Factory1 factory = new DXGI.Factory1())
            {
                int adapterCount = factory.GetAdapterCount();
                List<GraphicsAdapter> adapterList = new List<GraphicsAdapter>(adapterCount);

                for (int i = 0; i < adapterCount; i++)
                {
                    DXGI.Adapter1 dxAdapter = factory.GetAdapter1(i);

                    int monitorCount = dxAdapter.GetOutputCount();
                    for (int j = 0; j < monitorCount; j++)
                    {
                        using (DXGI.Output dxMonitor = dxAdapter.GetOutput(j))
                        {
                            ConcreteGraphicsAdapter adapterStrategy = new ConcreteGraphicsAdapter(dxAdapter, dxMonitor);
                            GraphicsAdapter adapter = base.CreateGraphicsAdapter(adapterStrategy);

                            adapterList.Add(adapter);
                        }
                    }
                }

                // The first adapter is considered the default.
                ((IPlatformGraphicsAdapter)adapterList[0]).Strategy.Platform_IsDefaultAdapter = true;

                return adapterList;
            }
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
