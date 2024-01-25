// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        private DXGI.Adapter1 _dxAdapter;
        private DisplayModeCollection _supportedDisplayModes;
        private DisplayMode _currentDisplayMode;

        public override DisplayModeCollection Platform_SupportedDisplayModes
        {
            get { return _supportedDisplayModes; }
        }

        public override DisplayMode Platform_CurrentDisplayMode
        {
            get { return _currentDisplayMode; }
        }

        public override bool Platform_IsWideScreen
        {
            // Seems like XNA treats aspect ratios above 16:10 as wide screen.
            get { return Platform_CurrentDisplayMode.AspectRatio >= (16.0f / 10.0f); }
        }

        private static readonly Dictionary<DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<DXGI.Format, SurfaceFormat>
        {
            { DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
            { DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
            { DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
        };

        internal ConcreteGraphicsAdapter(DXGI.Adapter1 dxAdapter, DXGI.Output dxMonitor)
        {
            _dxAdapter = dxAdapter;

            this.Platform_DeviceName = dxMonitor.Description.DeviceName.TrimEnd(new char[] { '\0' });
            this.Platform_Description = dxAdapter.Description1.Description.TrimEnd(new char[] { '\0' });
            this.Platform_DeviceId = dxAdapter.Description1.DeviceId;
            this.Platform_Revision = dxAdapter.Description1.Revision;
            this.Platform_VendorId = dxAdapter.Description1.VendorId;
            this.Platform_SubSystemId = dxAdapter.Description1.SubsystemId;
            this.Platform_MonitorHandle = dxMonitor.Description.MonitorHandle;

            int desktopWidth  = dxMonitor.Description.DesktopBounds.Right  - dxMonitor.Description.DesktopBounds.Left;
            int desktopHeight = dxMonitor.Description.DesktopBounds.Bottom - dxMonitor.Description.DesktopBounds.Top;

            List<DisplayMode> modes = new List<DisplayMode>();

            foreach (KeyValuePair<DXGI.Format, SurfaceFormat> formatTranslation in FormatTranslations)
            {
                DXGI.ModeDescription[] displayModes;

                // This can fail on headless machines, so just assume the desktop size
                // is a valid mode and return that... so at least our unit tests work.
                try
                {
                    displayModes = dxMonitor.GetDisplayModeList(formatTranslation.Key, 0);
                }
                catch (DX.SharpDXException)
                {
                    DisplayMode mode = base.CreateDisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);

                    modes.Add(mode);
                    _currentDisplayMode = mode;
                    break;
                }


                foreach (DXGI.ModeDescription dxModeDesc in displayModes)
                {
                    DisplayMode mode = base.CreateDisplayMode(dxModeDesc.Width, dxModeDesc.Height, formatTranslation.Value);

                    // Skip duplicate modes with the same width/height/formats.
                    if (modes.Contains(mode))
                        continue;

                    modes.Add(mode);

                    if (_currentDisplayMode == null)
                    {
                        if (mode.Width == desktopWidth && mode.Height == desktopHeight && mode.Format == SurfaceFormat.Color)
                            _currentDisplayMode = mode;
                    }
                }
            }

            _supportedDisplayModes = base.CreateDisplayModeCollection(modes);

            if (_currentDisplayMode == null) //(i.e. desktop mode wasn't found in the available modes)
            {
                _currentDisplayMode = base.CreateDisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);
            }

            return;
        }

        public override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (GraphicsAdapter.UseReferenceDevice)
                return true;

            D3D.FeatureLevel highestSupportedLevel;
            try
            {
                highestSupportedLevel = D3D11.Device.GetSupportedFeatureLevel(_dxAdapter);
            }
            catch (DX.SharpDXException ex)
            {
                if (ex.ResultCode == DXGI.ResultCode.Unsupported) // No supported feature levels!
                    return false;
                throw;
            }

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_9_1);
                case GraphicsProfile.HiDef:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_9_3);
                case GraphicsProfile.FL10_0:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_10_0);
                case GraphicsProfile.FL10_1:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_10_1);
                case GraphicsProfile.FL11_0:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_11_0);
                case GraphicsProfile.FL11_1:
                    return (highestSupportedLevel >= D3D.FeatureLevel.Level_11_1);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        public override bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
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
