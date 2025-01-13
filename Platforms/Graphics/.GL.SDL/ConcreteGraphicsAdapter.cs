// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using GetParamName = Microsoft.Xna.Platform.Graphics.OpenGL.GetPName;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        private DisplayModeCollection _supportedDisplayModes;
        private DisplayMode _currentDisplayMode;
        private string _description = string.Empty;

        int _displayIndex;


        public override string Platform_DeviceName
        {
            get { throw new NotImplementedException(); }
        }

        public override string Platform_Description
        {
            get { return _description; }
            set { }
        }

        public override int Platform_DeviceId
        {
            get { throw new NotImplementedException(); }
        }

        public override int Platform_Revision
        {
            get { throw new NotImplementedException(); }
        }

        public override int Platform_VendorId
        {
            get { throw new NotImplementedException(); }
        }

        public override int Platform_SubSystemId
        {
            get { throw new NotImplementedException(); }
        }

        public override IntPtr Platform_MonitorHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override bool Platform_IsDefaultAdapter
        {
            get { return base.Platform_IsDefaultAdapter; }
            set { base.Platform_IsDefaultAdapter = value; }
        }

        public override DisplayModeCollection Platform_SupportedDisplayModes
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
                        var displayMode = base.CreateDisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
                        if (!modes.Contains(displayMode))
                            modes.Add(displayMode);
                    }
                    modes.Sort(delegate (DisplayMode a, DisplayMode b)
                    {
                        if (a == b) return 0;
                        if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) return -1;
                        else return 1;
                    });
                    _supportedDisplayModes = base.CreateDisplayModeCollection(modes);
                }

                return _supportedDisplayModes;
            }
        }

        public override DisplayMode Platform_CurrentDisplayMode
        {
            get
            {
                int displayIndex = SDL.DISPLAY.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                SDL.DISPLAY.GetCurrentDisplayMode(displayIndex, out mode);

                _currentDisplayMode = base.CreateDisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);

                return _currentDisplayMode;
            }
        }

        public override bool Platform_IsWideScreen
        {
            // Common non-widescreen modes: 4:3, 5:4, 1:1
            // Common widescreen modes: 16:9, 16:10, 2:1
            // XNA does not appear to account for rotated displays on the desktop
            get { return Platform_CurrentDisplayMode.AspectRatio > (4.0f / 3.0f); }
        }

        public override GraphicsBackend Backend
        {
            get { return GraphicsBackend.OpenGL; }
        }


        private OGL _gl;
        private string _version;
        private GLVersion _glVersion;

        int _capMaxTextureSize;
        int _capMaxMultiSampleCount;
        int _capMaxTextureSlots;
        int _capMaxVertexTextureSlots;
        int _capMaxVertexAttribs;
        int _capMaxDrawBuffers;

        internal OGL GL { get { return _gl; } }
        internal GLVersion glVersion { get { return _glVersion; } }


        internal ConcreteGraphicsAdapter()
        {
            IntPtr glWindowHandle = IntPtr.Zero;
            IntPtr glContext = IntPtr.Zero;
            try
            {
                SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
                SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 0);

                glWindowHandle = SDL.WINDOW.Create("KnisDefaultAdapterWindow", 0, 0, 0, 0,
                    Sdl.Window.State.Hidden | Sdl.Window.State.OpenGL);
                glContext = SDL.OpenGL.CreateGLContext(glWindowHandle);

                try
                {
                    // OGL.Initialize() must be called while we have a gl context,
                    // because of OGL.LoadEntryPoints() & and OGL.InitExtensions().
                    OGL_SDL.Initialize();
                    OGL_SDL.Current.InitExtensions();
                    _gl = OGL.Current;
                }
                catch (EntryPointNotFoundException)
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions. " +
                        "Try updating your graphics drivers.");
                }

                // try getting the context version
                // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on GL_VERSION string.
                try
                {
                    _version = _gl.GetString(StringName.Version);
                    if (string.IsNullOrEmpty(_version))
                        throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                    // for OpenGL, the GL_VERSION string always starts with the version number in the "major.minor" format,
                    // optionally followed by multiple vendor specific characters
                    _glVersion.Major = Convert.ToInt16(_version.Substring(0, 1));
                    _glVersion.Minor = Convert.ToInt16(_version.Substring(2, 1));
                }
                catch (FormatException)
                {
                    // if it fails, we assume to be on a 1.1 context
                    _glVersion = new GLVersion(1, 1);
                }

                _description = _gl.GetString(StringName.Renderer);

                // get adapter caps.
                _gl.GetInteger(GetParamName.MaxTextureSize, out _capMaxTextureSize);
                _gl.CheckGLError();
                _gl.GetInteger(GetParamName.MaxSamples, out _capMaxMultiSampleCount);
                _gl.CheckGLError();
                _gl.GetInteger(GetParamName.MaxTextureImageUnits, out _capMaxTextureSlots);
                _gl.CheckGLError();
                _gl.GetInteger(GetParamName.MaxVertexTextureImageUnits, out _capMaxVertexTextureSlots);
                _gl.CheckGLError();
                _gl.GetInteger(GetParamName.MaxVertexAttribs, out _capMaxVertexAttribs);
                _gl.CheckGLError();
                _gl.GetInteger(GetParamName.MaxDrawBuffers, out _capMaxDrawBuffers);
                _gl.CheckGLError();

                int maxCombinedTextureImageUnits;
                _gl.GetInteger(GetParamName.MaxCombinedTextureImageUnits, out maxCombinedTextureImageUnits);
                _gl.CheckGLError();
                _capMaxTextureSlots = Math.Min(_capMaxTextureSlots, maxCombinedTextureImageUnits);
                _capMaxVertexTextureSlots = Math.Min(_capMaxVertexTextureSlots, maxCombinedTextureImageUnits);

            }
            finally
            {
                if (glContext != IntPtr.Zero)
                    SDL.OpenGL.DeleteContext(glContext);
                if (glWindowHandle != IntPtr.Zero)
                    SDL.WINDOW.Destroy(glWindowHandle);
            }
        }

        public override bool Platform_IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if (GraphicsAdapter.UseReferenceDevice)
                return true;

            switch (graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                case GraphicsProfile.HiDef:
                    if (_capMaxTextureSize >= 4096) return true;
                    return false;
                case GraphicsProfile.FL10_0:
                    if (_capMaxTextureSize >= 8192) return true;
                    return false;
                case GraphicsProfile.FL10_1:
                case GraphicsProfile.FL11_0:
                    if (_capMaxTextureSize >= 16384) return true;
                    return false;
                case GraphicsProfile.FL11_1:
                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool Platform_QueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount,
            out SurfaceFormat selectedFormat, out DepthFormat selectedDepthFormat, out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }
        
        public override bool Platform_QueryRenderTargetFormat(
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
