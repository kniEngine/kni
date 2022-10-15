// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Microsoft.Xna.Framework.Graphics
{
    class ConcreteGraphicsAdaptersProvider : GraphicsAdaptersProviderStrategy
    {

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_InitializeAdapters()
        {
            throw new PlatformNotSupportedException();
        }

        internal override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { throw new PlatformNotSupportedException(); }
        }

        internal override GraphicsAdapter Platform_DefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }
    }

    partial class GraphicsAdapter : GraphicsAdapterStrategy
    {
        override internal string Platform_DeviceName
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal string Platform_Description
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal int Platform_DeviceId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal int Platform_Revision
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal int Platform_VendorId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal int Platform_SubSystemId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal IntPtr Platform_MonitorHandle
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal bool Platform_IsDefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal DisplayModeCollection Platform_SupportedDisplayModes
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal DisplayMode Platform_CurrentDisplayMode
        {
            get { throw new PlatformNotSupportedException(); }
        }

        override internal bool Platform_IsWideScreen
        {
            get { throw new PlatformNotSupportedException(); }
        }

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
