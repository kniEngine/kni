// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformGraphicsAdapter
    {
        GraphicsAdapterStrategy Strategy { get; }
    }

    public abstract class GraphicsAdapterStrategy
    {
        public virtual string Platform_DeviceName { get; set; }
        public virtual string Platform_Description { get; set; }
        public virtual int Platform_DeviceId { get; set; }
        public virtual int Platform_Revision { get; set; }
        public virtual int Platform_VendorId { get; set; }
        public virtual int Platform_SubSystemId { get; set; }
        public virtual IntPtr Platform_MonitorHandle { get; set; }
        public virtual bool Platform_IsDefaultAdapter { get; set; }

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

        public T ToConcrete<T>() where T : GraphicsAdapterStrategy
        {
            return (T)this;
        }

        protected DisplayMode CreateDisplayMode(int width, int height, SurfaceFormat format)
        {
            return new DisplayMode(width, height, format);
        }

        protected DisplayModeCollection CreateDisplayModeCollection(List<DisplayMode> modes)
        {
            return new DisplayModeCollection(modes);
        }
    }
}
