// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class GraphicsAdapter
    {

        internal GraphicsAdapter()
        {
            throw new PlatformNotSupportedException();
        }

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public DisplayMode CurrentDisplayMode
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public static GraphicsAdapter DefaultAdapter
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

        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            throw new PlatformNotSupportedException();
        }

        public bool QueryBackBufferFormat(
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

        /// <summary>
        /// Queries for support of the requested render target format on the adaptor.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="format">The requested surface format.</param>
        /// <param name="depthFormat">The requested depth stencil format.</param>
        /// <param name="multiSampleCount">The requested multisample count.</param>
        /// <param name="selectedFormat">Set to the best format supported by the adaptor for the requested surface format.</param>
        /// <param name="selectedDepthFormat">Set to the best format supported by the adaptor for the requested depth stencil format.</param>
        /// <param name="selectedMultiSampleCount">Set to the best count supported by the adaptor for the requested multisample count.</param>
        /// <returns>True if the requested format is supported by the adaptor. False if one or more of the values was changed.</returns>
		public bool QueryRenderTargetFormat(
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
