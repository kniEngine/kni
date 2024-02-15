// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        public override string Platform_DeviceName
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override string Platform_Description
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int Platform_DeviceId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int Platform_Revision
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int Platform_VendorId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override int Platform_SubSystemId
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override IntPtr Platform_MonitorHandle
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool Platform_IsDefaultAdapter
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override DisplayModeCollection Platform_SupportedDisplayModes
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override DisplayMode Platform_CurrentDisplayMode
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool Platform_IsWideScreen
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override GraphicsBackend Backend
        {
            get { throw new PlatformNotSupportedException(); }
        }

        internal ConcreteGraphicsAdapter()
        {
        }

        public override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
