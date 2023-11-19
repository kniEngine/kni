// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
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

        public ConcreteGraphicsAdapter()
        {
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
