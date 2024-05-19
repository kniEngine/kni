// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Android.Views;
using Android.Runtime;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using GetParamName = Microsoft.Xna.Platform.Graphics.OpenGL.GetPName;
using Javax.Microedition.Khronos.Egl;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteGraphicsAdapter : GraphicsAdapterStrategy
    {
        private DisplayModeCollection _supportedDisplayModes;
        private DisplayMode _currentDisplayMode;
        private string _description = string.Empty;



        public override string Platform_DeviceName
        {
            get { throw new NotImplementedException(); }
        }

        public override string Platform_Description
        {
            get { return _description; }
            set { _description = value; }
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
                if (_supportedDisplayModes == null || displayChanged)
                {
                    var modes = new List<DisplayMode>(new[] { Platform_CurrentDisplayMode, });

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
                View view = ((AndroidGameWindow)ConcreteGame.GameConcreteInstance.Window).GameView;
                _currentDisplayMode = base.CreateDisplayMode(view.Width, view.Height, SurfaceFormat.Color);

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
            get { return GraphicsBackend.GLES; }
        }

        OGL_DROID _ogl;
        EGLDisplay _eglDisplay;

        internal OGL_DROID Ogl { get { return _ogl; } }
        internal EGLDisplay EglDisplay { get { return _eglDisplay; } }

        internal ConcreteGraphicsAdapter()
        {
            if (OGL_DROID.Current == null)
                OGL_DROID.Initialize();

            _ogl = (OGL_DROID)OGL.Current;

#if CARDBOARD
            _eglDisplay = _ogl.Egl.EglGetCurrentDisplay();
#else
            _eglDisplay = _ogl.Egl.EglGetDisplay(EGL10.EglDefaultDisplay);
            if (_eglDisplay == EGL10.EglNoDisplay)
                throw new Exception("Could not get EGL display" + _ogl.GetEglErrorAsString());

            int[] version = new int[2];
            if (!_ogl.Egl.EglInitialize(_eglDisplay, version))
                throw new Exception("Could not initialize EGL display" + _ogl.GetEglErrorAsString());
#endif
        }

        ~ConcreteGraphicsAdapter()
        {
#if CARDBOARD
#else
            if (_eglDisplay != null)
            {
                if (!_ogl.Egl.EglTerminate(_eglDisplay))
                    throw new Exception("Could not terminate EGL connection" + _ogl.GetEglErrorAsString());
            }
            _eglDisplay = null;
#endif
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
                    int maxTextureSize;
                    _ogl.GetInteger(GetParamName.MaxTextureSize, out maxTextureSize);                    
                    if (maxTextureSize >= 4096) return true;
                    return false;
                case GraphicsProfile.FL10_0:
                    int maxTextureSize2;
                    _ogl.GetInteger(GetParamName.MaxTextureSize, out maxTextureSize2);                    
                    if (maxTextureSize2 >= 8192) return true;
                    return false;
                case GraphicsProfile.FL10_1:
                    int maxVertexBufferSlots;
                    _ogl.GetInteger(GetParamName.MaxVertexAttribs, out maxVertexBufferSlots);
                    if (maxVertexBufferSlots >= 32) return true;
                    return false;
                case GraphicsProfile.FL11_0:
                    int maxTextureSize3;
                    _ogl.GetInteger(GetParamName.MaxTextureSize, out maxTextureSize3);                    
                    if (maxTextureSize3 >= 16384) return true;
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
