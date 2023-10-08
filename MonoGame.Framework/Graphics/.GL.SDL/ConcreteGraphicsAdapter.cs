// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        private DisplayModeCollection _supportedDisplayModes;
        string _description = string.Empty;

        int _displayIndex;


        override internal string Platform_DeviceName
        {
            get { throw new NotImplementedException(); }
        }

        override internal string Platform_Description
        {
            get
            {
                try
                {
                    var GL = OGL.Current;
                    return GL.GetString(OpenGL.StringName.Renderer);
                }
                catch { return string.Empty; }
            }
            set { }
        }

        override internal int Platform_DeviceId
        {
            get { throw new NotImplementedException(); }
        }

        override internal int Platform_Revision
        {
            get { throw new NotImplementedException(); }
        }

        override internal int Platform_VendorId
        {
            get { throw new NotImplementedException(); }
        }

        override internal int Platform_SubSystemId
        {
            get { throw new NotImplementedException(); }
        }

        override internal IntPtr Platform_MonitorHandle
        {
            get { throw new NotImplementedException(); }
        }

        override internal bool Platform_IsDefaultAdapter
        {
            get { throw new NotImplementedException(); }
        }

        override internal DisplayModeCollection Platform_SupportedDisplayModes
        {
            get
            {
                bool displayChanged = false;
                var displayIndex = SDL.DISPLAY.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);
                displayChanged = displayIndex != _displayIndex;
                if (_supportedDisplayModes == null || displayChanged)
                {
                    var modes = new List<DisplayMode>(new[] { Platform_CurrentDisplayMode, });
                    
                    _displayIndex = displayIndex;
                    modes.Clear();

                    var modeCount = SDL.DISPLAY.GetNumDisplayModes(displayIndex);

                    for (int i = 0; i < modeCount; i++)
                    {
                        Sdl.Display.Mode mode;
                        SDL.DISPLAY.GetDisplayMode(displayIndex, i, out mode);

                        // We are only using one format, Color
                        // mode.Format gets the Color format from SDL
                        var displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
                        if (!modes.Contains(displayMode))
                            modes.Add(displayMode);
                    }
                    modes.Sort(delegate (DisplayMode a, DisplayMode b)
                    {
                        if (a == b) return 0;
                        if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) return -1;
                        else return 1;
                    });
                    _supportedDisplayModes = new DisplayModeCollection(modes);
                }

                return _supportedDisplayModes;
            }
        }

        override internal DisplayMode Platform_CurrentDisplayMode
        {
            get
            {
                var displayIndex = SDL.DISPLAY.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                SDL.DISPLAY.GetCurrentDisplayMode(displayIndex, out mode);

                return new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
            }
        }

        override internal bool Platform_IsWideScreen
        {
            // Common non-widescreen modes: 4:3, 5:4, 1:1
            // Common widescreen modes: 16:9, 16:10, 2:1
            // XNA does not appear to account for rotated displays on the desktop
            get { return Platform_CurrentDisplayMode.AspectRatio > (4.0f / 3.0f); }
        }

        internal override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (GraphicsAdapter.UseReferenceDevice)
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
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }
        
        internal override bool Platform_QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
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

    }
}
