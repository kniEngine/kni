// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        internal DXGI.Adapter1 _adapter;
        internal DisplayModeCollection _supportedDisplayModes;
        internal DisplayMode _currentDisplayMode;

        override internal DisplayModeCollection Platform_SupportedDisplayModes
        {
            get { return _supportedDisplayModes; }
        }

        override internal DisplayMode Platform_CurrentDisplayMode
        {
            get { return _currentDisplayMode; }
        }

        override internal bool Platform_IsWideScreen
        {
            // Seems like XNA treats aspect ratios above 16:10 as wide screen.
            get { return Platform_CurrentDisplayMode.AspectRatio >= (16.0f / 10.0f); }
        }

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (GraphicsAdapter.UseReferenceDevice)
                return true;

            D3D.FeatureLevel highestSupportedLevel;
            try
            {
                highestSupportedLevel = D3D11.Device.GetSupportedFeatureLevel(_adapter);
            }
            catch (SharpDX.SharpDXException ex)
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

        internal override bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        internal override bool Platform_QueryRenderTargetFormat(
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
