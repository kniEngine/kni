// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class GraphicsAdapter : GraphicsAdapterStrategy
    {

        private static ReadOnlyCollection<GraphicsAdapter> Platform_InitializeAdapters()
        {
            throw new PlatformNotSupportedException();
        }

        public static ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public static GraphicsAdapter Platform_DefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get { throw new PlatformNotSupportedException(); }
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



        public DisplayMode CurrentDisplayMode
        {
            get { throw new PlatformNotSupportedException(); }
        }


        public string Description
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int DeviceId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public string DeviceName
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public bool IsDefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> indicating whether
        /// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
        /// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </summary>
        public bool IsWideScreen
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public IntPtr MonitorHandle
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int Revision
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public int SubSystemId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public DisplayModeCollection SupportedDisplayModes
        {
            get { throw new PlatformNotSupportedException(); }
        }


        public int VendorId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        /*
        public Guid DeviceIdentifier
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public string DriverDll
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public Version DriverVersion
        {
            get { throw new PlatformNotSupportedException(); }
        }
        */

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            throw new PlatformNotSupportedException();
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
            throw new PlatformNotSupportedException();
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
            throw new PlatformNotSupportedException();
        }

    }
}
