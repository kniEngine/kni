// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsAdapter
        : IPlatformGraphicsAdapter
    {
        private GraphicsAdapterStrategy _strategy;

        GraphicsAdapterStrategy IPlatformGraphicsAdapter.Strategy { get { return _strategy; } }

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get { return GraphicsAdaptersProviderStrategy.Current.Platform_Adapters; }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return GraphicsAdaptersProviderStrategy.Current.Platform_DefaultAdapter; }
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
            get { return GraphicsAdaptersProviderStrategy.Current.Platform_UseReferenceDevice; }
            set { GraphicsAdaptersProviderStrategy.Current.Platform_UseReferenceDevice = value; }
        }

        public string DeviceName
        {
            get { return _strategy.Platform_DeviceName; }
        }

        public string Description
        {
            get { return _strategy.Platform_Description; }
        }

        public int DeviceId
        {
            get { return _strategy.Platform_DeviceId; }
        }

        public int Revision
        {
            get { return _strategy.Platform_Revision; }
        }

        public int VendorId
        {
            get { return _strategy.Platform_VendorId; }
        }

        public int SubSystemId
        {
            get { return _strategy.Platform_SubSystemId; }
        }
        public IntPtr MonitorHandle
        {
            get { return _strategy.Platform_MonitorHandle; }
        }

        public bool IsDefaultAdapter
        {
            get { return _strategy.Platform_IsDefaultAdapter; }
        }

        public DisplayModeCollection SupportedDisplayModes
        {
            get { return _strategy.Platform_SupportedDisplayModes; }
        }

        public DisplayMode CurrentDisplayMode
        {
            get { return _strategy.Platform_CurrentDisplayMode; }
        }

        /// <summary>
        /// Returns true if the <see cref="GraphicsAdapter.CurrentDisplayMode"/> is widescreen.
        /// </summary>
        /// <remarks>
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </remarks>
        public bool IsWideScreen
        {
            get { return _strategy.Platform_IsWideScreen; }
        }

        public GraphicsBackend Backend
        {
            get { return _strategy.Backend; }
        }

        internal GraphicsAdapter(GraphicsAdapterStrategy strategy)
        {
            _strategy = strategy;
        }

        public bool QueryBackBufferFormat(
             GraphicsProfile graphicsProfile,
             SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
             out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            return _strategy.Platform_QueryBackBufferFormat(
                graphicsProfile,
                format, depthFormat, multiSampleCount,
                out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount
            );
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
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            return _strategy.Platform_QueryRenderTargetFormat(
                graphicsProfile,
                format, depthFormat, multiSampleCount,
                out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount
            );
        }

        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            return _strategy.Platform_IsProfileSupported(graphicsProfile);
        }

    }
}
