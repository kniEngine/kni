// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpDX.Direct3D;


namespace Microsoft.Xna.Framework.Graphics
{
    class ConcreteGraphicsAdaptersProvider : GraphicsAdaptersProviderStrategy
    {
        private ReadOnlyCollection<GraphicsAdapter> _adapters;

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_InitializeAdapters()
        {
            // NOTE: An adapter is a monitor+device combination, so we expect
            // at lease one adapter per connected monitor.

            var factory = new SharpDX.DXGI.Factory1();

            var adapterCount = factory.GetAdapterCount();
            var adapterList = new List<GraphicsAdapter>(adapterCount);

            for (var i = 0; i < adapterCount; i++)
            {
                var device = factory.GetAdapter1(i);

                var monitorCount = device.GetOutputCount();
                for (var j = 0; j < monitorCount; j++)
                {
                    var monitor = device.GetOutput(j);

                    var adapter = CreateAdapter(device, monitor);
                    adapterList.Add(adapter);

                    monitor.Dispose();
                }
            }

            // The first adapter is considered the default.
            adapterList[0].IsDefaultAdapter = true;

            factory.Dispose();

            return new ReadOnlyCollection<GraphicsAdapter>(adapterList);
        }

        private static readonly Dictionary<SharpDX.DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<SharpDX.DXGI.Format, SurfaceFormat>
        {
            { SharpDX.DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
        };

        private GraphicsAdapter CreateAdapter(SharpDX.DXGI.Adapter1 device, SharpDX.DXGI.Output monitor)
        {
            var adapter = new GraphicsAdapter();
            adapter._adapter = device;

            adapter.DeviceName = monitor.Description.DeviceName.TrimEnd(new char[] { '\0' });
            adapter.Strategy.Platform_Description = device.Description1.Description.TrimEnd(new char[] { '\0' });
            adapter.DeviceId = device.Description1.DeviceId;
            adapter.Revision = device.Description1.Revision;
            adapter.VendorId = device.Description1.VendorId;
            adapter.SubSystemId = device.Description1.SubsystemId;
            adapter.MonitorHandle = monitor.Description.MonitorHandle;

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
                    adapter._currentDisplayMode = mode;
                    break;
                }


                foreach (var displayMode in displayModes)
                {
                    var mode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);

                    // Skip duplicate modes with the same width/height/formats.
                    if (modes.Contains(mode))
                        continue;

                    modes.Add(mode);

                    if (adapter._currentDisplayMode == null)
                    {
                        if (mode.Width == desktopWidth && mode.Height == desktopHeight && mode.Format == SurfaceFormat.Color)
                            adapter._currentDisplayMode = mode;
                    }
                }
            }

            adapter._supportedDisplayModes = new DisplayModeCollection(modes);

            if (adapter._currentDisplayMode == null) //(i.e. desktop mode wasn't found in the available modes)
                adapter._currentDisplayMode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);

            return adapter;
        }

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get
            {
                if (_adapters == null)
                {
                    _adapters = Platform_InitializeAdapters();
                }

                return _adapters;
            }
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

    partial class GraphicsAdapter : GraphicsAdapterStrategy
    {
        /// <summary>
        /// Defines the driver type for graphics adapter.
        /// </summary>
        /// <remarks>Usable only on DirectX platforms.</remarks>
        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering. Maximum speed and performance.
            /// </summary>
            Hardware,
            /// <summary>
            /// Emulates the hardware device on CPU. Slowly, only for testing.
            /// </summary>
            Reference,
            /// <summary>
            /// Useful when <see cref="DriverType.Hardware"/> acceleration does not work.
            /// </summary>
            FastSoftware
        }

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public static DriverType UseDriverType
        {
            get { return ((ConcreteGraphicsAdaptersProvider)GraphicsAdaptersProviderStrategy.Current).PlatformDX_UseDriverType; }
            set { ((ConcreteGraphicsAdaptersProvider)GraphicsAdaptersProviderStrategy.Current).PlatformDX_UseDriverType = value; }
        }

        /// <summary>
        /// Used to request the graphics device should be created with debugging
        /// features enabled.
        /// </summary>
        /// <remarks>Usable only on DirectX platforms.</remarks>
        public static bool UseDebugLayers
        {
            get { return ((ConcreteGraphicsAdaptersProvider)GraphicsAdaptersProviderStrategy.Current).PlatformDX_UseDebugLayers; }
            set { ((ConcreteGraphicsAdaptersProvider)GraphicsAdaptersProviderStrategy.Current).PlatformDX_UseDebugLayers = value; }
        }

        public int DeviceId { get; internal set; }

        public string DeviceName { get; internal set; }

        public int VendorId { get; internal set; }

        public bool IsDefaultAdapter { get; internal set; }

        public IntPtr MonitorHandle { get; internal set; }

        public int Revision { get; internal set; }

        public int SubSystemId { get; internal set; }

        public DisplayModeCollection SupportedDisplayModes
        {
            get { return _supportedDisplayModes; }
        }

        public DisplayMode CurrentDisplayMode
        {
            get { return _currentDisplayMode; }
        }

        /// <summary>
        /// Returns true if the <see cref="GraphicsAdapter.CurrentDisplayMode"/> is widescreen.
        /// </summary>
        /// <remarks>
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </remarks>
        public bool IsWideScreen
        {
            get
            {
                // Seems like XNA treats aspect ratios above 16:10 as wide screen.
                const float minWideScreenAspect = 16.0f / 10.0f;
                return CurrentDisplayMode.AspectRatio >= minWideScreenAspect;
            }
        }




        internal DisplayModeCollection _supportedDisplayModes;
        internal DisplayMode _currentDisplayMode;
        internal SharpDX.DXGI.Adapter1 _adapter;

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (UseReferenceDevice)
                return true;

            FeatureLevel highestSupportedLevel;
            try
            {
                highestSupportedLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(_adapter);
            }
            catch (SharpDX.SharpDXException ex)
            {
                if (ex.ResultCode == SharpDX.DXGI.ResultCode.Unsupported) // No supported feature levels!
                    return false;
                throw;
            }

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return (highestSupportedLevel >= FeatureLevel.Level_9_1);
                case GraphicsProfile.HiDef:
                    return (highestSupportedLevel >= FeatureLevel.Level_9_3);
                case GraphicsProfile.FL10_0:
                    return (highestSupportedLevel >= FeatureLevel.Level_10_0);
                case GraphicsProfile.FL10_1:
                    return (highestSupportedLevel >= FeatureLevel.Level_10_1);
                case GraphicsProfile.FL11_0:
                    return (highestSupportedLevel >= FeatureLevel.Level_11_0);
                case GraphicsProfile.FL11_1:
                    return (highestSupportedLevel >= FeatureLevel.Level_11_1);
                default:
                    throw new InvalidOperationException();
            }
        }

        internal override bool Platform_QueryBackBufferFormat(
             GraphicsProfile graphicsProfile,
             SurfaceFormat format,
             DepthFormat depthFormat,
             int multiSampleCount,
             out SurfaceFormat selectedFormat,
             out DepthFormat selectedDepthFormat,
             out int selectedMultiSampleCount
            )
        {
            throw new NotImplementedException();
        }

        internal override bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            selectedFormat = format;
            selectedDepthFormat = depthFormat;
            selectedMultiSampleCount = multiSampleCount;

            // fallback for unsupported renderTarget surface formats.
            if (selectedFormat == SurfaceFormat.Alpha8 ||
                selectedFormat == SurfaceFormat.NormalizedByte2 ||
                selectedFormat == SurfaceFormat.NormalizedByte4 ||
                selectedFormat == SurfaceFormat.Dxt1 ||
                selectedFormat == SurfaceFormat.Dxt3 ||
                selectedFormat == SurfaceFormat.Dxt5 ||
                selectedFormat == SurfaceFormat.Dxt1a ||
                selectedFormat == SurfaceFormat.Dxt1SRgb ||
                selectedFormat == SurfaceFormat.Dxt3SRgb ||
                selectedFormat == SurfaceFormat.Dxt5SRgb)
                selectedFormat = SurfaceFormat.Color;

            // fallback for unsupported renderTarget surface formats on Reach profile.
            if (graphicsProfile == GraphicsProfile.Reach)
            {
                if (selectedFormat == SurfaceFormat.HalfSingle ||
                    selectedFormat == SurfaceFormat.HalfVector2 ||
                    selectedFormat == SurfaceFormat.HalfVector4 ||
                    selectedFormat == SurfaceFormat.HdrBlendable ||
                    selectedFormat == SurfaceFormat.Rg32 ||
                    selectedFormat == SurfaceFormat.Rgba1010102 ||
                    selectedFormat == SurfaceFormat.Rgba64 ||
                    selectedFormat == SurfaceFormat.Single ||
                    selectedFormat == SurfaceFormat.Vector2 ||
                    selectedFormat == SurfaceFormat.Vector4)
                    selectedFormat = SurfaceFormat.Color;
            }

            return (format == selectedFormat) && (depthFormat == selectedDepthFormat) && (multiSampleCount == selectedMultiSampleCount);
        }

    }
}
