// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

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
            // NOTE: An adapter is a monitor+device combination, so we expect
            // at lease one adapter per connected monitor.

            using (DXGI.Factory1 factory = new DXGI.Factory1())
            {
                int adapterCount = factory.GetAdapterCount();
                List<GraphicsAdapter> adapterList = new List<GraphicsAdapter>(adapterCount);

                for (int i = 0; i < adapterCount; i++)
                {
                    DXGI.Adapter1 device = factory.GetAdapter1(i);

                    int monitorCount = device.GetOutputCount();
                    for (int j = 0; j < monitorCount; j++)
                    {
                        using (DXGI.Output monitor = device.GetOutput(j))
                        {
                            GraphicsAdapter adapter = CreateAdapter(device, monitor);
                            adapterList.Add(adapter);
                        }                        
                    }
                }

                // The first adapter is considered the default.
                ((IPlatformGraphicsAdapter)adapterList[0]).Strategy.Platform_IsDefaultAdapter = true;

                _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
            }

            return;
        }

        private static readonly Dictionary<DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<DXGI.Format, SurfaceFormat>
        {
            { DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
            { DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
            { DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
        };

        private GraphicsAdapter CreateAdapter(DXGI.Adapter1 device, DXGI.Output monitor)
        {
            ConcreteGraphicsAdapter strategy = new ConcreteGraphicsAdapter();
            GraphicsAdapter adapter = new GraphicsAdapter(strategy);
            strategy._adapter = device;

            strategy.Platform_DeviceName = monitor.Description.DeviceName.TrimEnd(new char[] { '\0' });
            strategy.Platform_Description = device.Description1.Description.TrimEnd(new char[] { '\0' });
            strategy.Platform_DeviceId = device.Description1.DeviceId;
            strategy.Platform_Revision = device.Description1.Revision;
            strategy.Platform_VendorId = device.Description1.VendorId;
            strategy.Platform_SubSystemId = device.Description1.SubsystemId;
            strategy.Platform_MonitorHandle = monitor.Description.MonitorHandle;

            int desktopWidth = monitor.Description.DesktopBounds.Right - monitor.Description.DesktopBounds.Left;
            int desktopHeight = monitor.Description.DesktopBounds.Bottom - monitor.Description.DesktopBounds.Top;

            var modes = new List<DisplayMode>();

            foreach (var formatTranslation in FormatTranslations)
            {
                DXGI.ModeDescription[] displayModes;

                // This can fail on headless machines, so just assume the desktop size
                // is a valid mode and return that... so at least our unit tests work.
                try
                {
                    displayModes = monitor.GetDisplayModeList(formatTranslation.Key, 0);
                }
                catch (DX.SharpDXException)
                {
                    DisplayMode mode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);
                    modes.Add(mode);
                    strategy._currentDisplayMode = mode;
                    break;
                }


                foreach (DXGI.ModeDescription displayMode in displayModes)
                {
                    DisplayMode mode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);

                    // Skip duplicate modes with the same width/height/formats.
                    if (modes.Contains(mode))
                        continue;

                    modes.Add(mode);

                    if (((IPlatformGraphicsAdapter)adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>()._currentDisplayMode == null)
                    {
                        if (mode.Width == desktopWidth && mode.Height == desktopHeight && mode.Format == SurfaceFormat.Color)
                            ((IPlatformGraphicsAdapter)adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>()._currentDisplayMode = mode;
                    }
                }
            }

            ((IPlatformGraphicsAdapter)adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>()._supportedDisplayModes = new DisplayModeCollection(modes);

            if (((IPlatformGraphicsAdapter)adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>()._currentDisplayMode == null) //(i.e. desktop mode wasn't found in the available modes)
                ((IPlatformGraphicsAdapter)adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>()._currentDisplayMode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);

            return adapter;
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
