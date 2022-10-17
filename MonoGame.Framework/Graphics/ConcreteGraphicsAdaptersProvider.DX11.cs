// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdaptersProvider : GraphicsAdaptersProviderStrategy
    {
        private ReadOnlyCollection<GraphicsAdapter> _adapters;

        public ConcreteGraphicsAdaptersProvider()
        {
            // NOTE: An adapter is a monitor+device combination, so we expect
            // at lease one adapter per connected monitor.

            using (var factory = new SharpDX.DXGI.Factory1())
            {
                var adapterCount = factory.GetAdapterCount();
                var adapterList = new List<GraphicsAdapter>(adapterCount);

                for (var i = 0; i < adapterCount; i++)
                {
                    var device = factory.GetAdapter1(i);

                    var monitorCount = device.GetOutputCount();
                    for (var j = 0; j < monitorCount; j++)
                    {
                        using (var monitor = device.GetOutput(j))
                        {
                            var adapter = CreateAdapter(device, monitor);
                            adapterList.Add(adapter);
                        }                        
                    }
                }

                // The first adapter is considered the default.
                adapterList[0].Strategy.Platform_IsDefaultAdapter = true;

                _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
            }

            return;
        }

        private static readonly Dictionary<SharpDX.DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<SharpDX.DXGI.Format, SurfaceFormat>
        {
            { SharpDX.DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
        };

        private GraphicsAdapter CreateAdapter(SharpDX.DXGI.Adapter1 device, SharpDX.DXGI.Output monitor)
        {
            var strategy = new ConcreteGraphicsAdapter();
            var adapter = new GraphicsAdapter(strategy);
            strategy._adapter = device;

            strategy.Platform_DeviceName = monitor.Description.DeviceName.TrimEnd(new char[] { '\0' });
            strategy.Platform_Description = device.Description1.Description.TrimEnd(new char[] { '\0' });
            strategy.Platform_DeviceId = device.Description1.DeviceId;
            strategy.Platform_Revision = device.Description1.Revision;
            strategy.Platform_VendorId = device.Description1.VendorId;
            strategy.Platform_SubSystemId = device.Description1.SubsystemId;
            strategy.Platform_MonitorHandle = monitor.Description.MonitorHandle;

            var desktopWidth = monitor.Description.DesktopBounds.Right - monitor.Description.DesktopBounds.Left;
            var desktopHeight = monitor.Description.DesktopBounds.Bottom - monitor.Description.DesktopBounds.Top;

            var modes = new List<DisplayMode>();

            foreach (var formatTranslation in FormatTranslations)
            {
                SharpDX.DXGI.ModeDescription[] displayModes;

                // This can fail on headless machines, so just assume the desktop size
                // is a valid mode and return that... so at least our unit tests work.
                try
                {
                    displayModes = monitor.GetDisplayModeList(formatTranslation.Key, 0);
                }
                catch (SharpDX.SharpDXException)
                {
                    var mode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);
                    modes.Add(mode);
                    strategy._currentDisplayMode = mode;
                    break;
                }


                foreach (var displayMode in displayModes)
                {
                    var mode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);

                    // Skip duplicate modes with the same width/height/formats.
                    if (modes.Contains(mode))
                        continue;

                    modes.Add(mode);

                    if (((ConcreteGraphicsAdapter)adapter.Strategy)._currentDisplayMode == null)
                    {
                        if (mode.Width == desktopWidth && mode.Height == desktopHeight && mode.Format == SurfaceFormat.Color)
                            ((ConcreteGraphicsAdapter)adapter.Strategy)._currentDisplayMode = mode;
                    }
                }
            }

            ((ConcreteGraphicsAdapter)adapter.Strategy)._supportedDisplayModes = new DisplayModeCollection(modes);

            if (((ConcreteGraphicsAdapter)adapter.Strategy)._currentDisplayMode == null) //(i.e. desktop mode wasn't found in the available modes)
                ((ConcreteGraphicsAdapter)adapter.Strategy)._currentDisplayMode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);

            return adapter;
        }

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { return _adapters; }
        }

        internal override GraphicsAdapter Platform_DefaultAdapter
        {
            get { return _adapters[0]; }
        }

        internal override bool Platform_UseReferenceDevice
        {
            get { return PlatformDX_UseDriverType == GraphicsAdapter.DriverType.Reference; }
            set { PlatformDX_UseDriverType = value ? GraphicsAdapter.DriverType.Reference : GraphicsAdapter.DriverType.Hardware; }
        }

        internal bool PlatformDX_UseDebugLayers { get; set; }

        internal GraphicsAdapter.DriverType PlatformDX_UseDriverType { get; set; }

    }
}
