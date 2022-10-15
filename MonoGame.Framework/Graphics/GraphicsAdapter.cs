// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class GraphicsAdapter
    {
        internal GraphicsAdapterStrategy Strategy { get { return this; } }

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

        public string Description
        {
            get { return Strategy.Platform_Description; }
        }

        internal GraphicsAdapter()
        {
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
            return Strategy.Platform_QueryBackBufferFormat(
             graphicsProfile,
             format,
             depthFormat,
             multiSampleCount,
             out selectedFormat,
             out selectedDepthFormat,
             out selectedMultiSampleCount
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
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            return Strategy.Platform_QueryRenderTargetFormat(
                    graphicsProfile,
                    format,
                    depthFormat,
                    multiSampleCount,
                    out selectedFormat,
                    out selectedDepthFormat,
                    out selectedMultiSampleCount
                );
        }

        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            return Strategy.Platform_IsProfileSupported(graphicsProfile);
        }

    }

    public abstract class GraphicsAdapterStrategy
    {
        virtual internal string Platform_Description { get; set; }

        abstract internal bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile);

        abstract internal bool Platform_QueryBackBufferFormat(
             GraphicsProfile graphicsProfile,
             SurfaceFormat format,
             DepthFormat depthFormat,
             int multiSampleCount,
             out SurfaceFormat selectedFormat,
             out DepthFormat selectedDepthFormat,
             out int selectedMultiSampleCount
            );

        abstract internal bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount);
    }

    public abstract class GraphicsAdaptersProviderStrategy
    {
        private static GraphicsAdaptersProviderStrategy _current;

        internal static GraphicsAdaptersProviderStrategy Current
        {
            get
            {
                lock (typeof(GraphicsAdaptersProviderStrategy))
                {
                    if (_current == null)
                        _current = new ConcreteGraphicsAdaptersProvider();

                    return _current;
                }
            }
        }

        abstract internal ReadOnlyCollection<GraphicsAdapter> Platform_InitializeAdapters();
        abstract internal ReadOnlyCollection<GraphicsAdapter> Platform_Adapters { get; }
        abstract internal GraphicsAdapter Platform_DefaultAdapter { get; }
        virtual internal bool Platform_UseReferenceDevice { get; set; }
    }
}
