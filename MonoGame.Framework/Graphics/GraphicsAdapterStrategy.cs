// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsAdapterStrategy
    {
        virtual internal string Platform_DeviceName { get; set; }
        virtual internal string Platform_Description { get; set; }
        virtual internal int Platform_DeviceId { get; set; }
        virtual internal int Platform_Revision { get; set; }
        virtual internal int Platform_VendorId { get; set; }
        virtual internal int Platform_SubSystemId { get; set; }
        virtual internal IntPtr Platform_MonitorHandle { get; set; }
        virtual internal bool Platform_IsDefaultAdapter { get; set; }

        public abstract DisplayModeCollection Platform_SupportedDisplayModes { get; }
        public abstract DisplayMode Platform_CurrentDisplayMode { get; }
        public abstract bool Platform_IsWideScreen { get; }

        public abstract bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile);

        public abstract bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount);

        public abstract bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount);
            
        internal T ToConcrete<T>() where T : GraphicsAdapterStrategy
        {
            return (T)this;
        }

    }
}
