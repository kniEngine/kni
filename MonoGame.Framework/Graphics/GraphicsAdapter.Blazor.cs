// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class GraphicsAdapter : GraphicsAdapterStrategy
    {
        private static ReadOnlyCollection<GraphicsAdapter> _adapters;
       
        private static ReadOnlyCollection<GraphicsAdapter> Platform_InitializeAdapters()
        {
            return new ReadOnlyCollection<GraphicsAdapter>(
                new[] { new GraphicsAdapter() });
        }

        public static ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
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

        public static GraphicsAdapter Platform_DefaultAdapter
        {
            get { return Adapters[0]; }
        }

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get
            {
                if (_adapters == null)
                {
                    _adapters = Platform_Adapters;
                }

                return _adapters;
            }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Platform_DefaultAdapter; }
        }

        /// <summary>
        /// Used to request creation of the reference graphics device, 
        /// or the default hardware accelerated device (when set to false).
        /// </summary>
        /// <remarks>
        /// This only works on DirectX platforms where a reference graphics
        /// device is available and must be defined before the graphics device
        /// is created. It defaults to false.
        /// </remarks>
        public static bool UseReferenceDevice
        {
            get { return UseDriverType == DriverType.Reference; }
            set { UseDriverType = value ? DriverType.Reference : DriverType.Hardware; }
        }

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public static DriverType UseDriverType { get; set; }


        public string Description
        {
            get { return _description; }
            private set { _description = value; }
        }

        public DisplayMode CurrentDisplayMode
        {
            get { return new DisplayMode(800, 600, SurfaceFormat.Color); }
        }


        /*
        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public int DeviceId
        {
            get { throw new NotImplementedException(); }
        }

        public Guid DeviceIdentifier
        {
            get { throw new NotImplementedException(); }
        }

        public string DeviceName
        {
            get { throw new NotImplementedException(); }
        }

        public string DriverDll
        {
            get { throw new NotImplementedException(); }
        }

        public Version DriverVersion
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDefaultAdapter
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWideScreen
        {
            get { throw new NotImplementedException(); }
        }

        public IntPtr MonitorHandle
        {
            get { throw new NotImplementedException(); }
        }

        public int Revision
        {
            get { throw new NotImplementedException(); }
        }

        public int SubSystemId
        {
            get { throw new NotImplementedException(); }
        }
        */

        public DisplayModeCollection SupportedDisplayModes
        {
            get { throw new NotImplementedException(); }
        }

        /*
        public int VendorId
        {
            get { throw new NotImplementedException(); }
        }
        */

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> indicating whether
        /// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
        /// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </summary>
        public bool IsWideScreen
        {
            get
            {
                // Common non-widescreen modes: 4:3, 5:4, 1:1
                // Common widescreen modes: 16:9, 16:10, 2:1
                // XNA does not appear to account for rotated displays on the desktop
                const float limit = 4.0f / 3.0f;
                var aspect = CurrentDisplayMode.AspectRatio;
                return aspect > limit;
            }
        }

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (UseReferenceDevice)
                return true;

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                case GraphicsProfile.HiDef:
                    return false;
                case GraphicsProfile.FL10_0:
                    return false;
                case GraphicsProfile.FL10_1:
                    return false;
                case GraphicsProfile.FL11_0:
                    return false;
                case GraphicsProfile.FL11_1:
                    return false;
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


       private DisplayModeCollection _supportedDisplayModes;
        string _description = string.Empty;

    }
}
