// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class GL
    {
        private Sdl _sdl;

        public enum Attribute
        {
            RedSize,
            GreenSize,
            BlueSize,
            AlphaSize,
            BufferSize,
            DoubleBuffer,
            DepthSize,
            StencilSize,
            AccumRedSize,
            AccumGreenSize,
            AccumBlueSize,
            AccumAlphaSize,
            Stereo,
            MultiSampleBuffers,
            MultiSampleSamples,
            AcceleratedVisual,
            RetainedBacking,
            ContextMajorVersion,
            ContextMinorVersion,
            ContextEgl,
            ContextFlags,
            ContextProfileMask,
            ShareWithCurrentContext,
            FramebufferSRGBCapable,
            ContextReleaseBehaviour,
        }

        public enum ContextProfile : int
        {
            /// <summary>leaves the choice of profile up to SDL</summary>
            Default = 0,
            Core = 1,
            Compatibility = 2,
            ES = 3,
        }

        [Flags]
        public enum ContextFlag : int
        {
            Debug             = 1,
            ForwardCompatible = 2,
            RobustAccess      = 4,
            ResetIsolation    = 8,
        }

        public enum ContextReleaseBehaviour : int
        {
            None  = 0,
            Flush = 1,
        }

        public GL(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_createcontext(IntPtr window);
        private d_sdl_gl_createcontext SDL_GL_CreateContext;

        public IntPtr CreateGLContext(IntPtr window)
        {
            IntPtr pointer = SDL_GL_CreateContext(window);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_deletecontext(IntPtr context);
        public d_sdl_gl_deletecontext DeleteContext;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_getcurrentcontext();
        private d_sdl_gl_getcurrentcontext SDL_GL_GetCurrentContext;

        public IntPtr GetCurrentContext()
        {
            IntPtr pointer = SDL_GL_GetCurrentContext();
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_gl_getprocaddress(string proc);
        public d_sdl_gl_getprocaddress GetProcAddress;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_getswapinterval();
        public d_sdl_gl_getswapinterval GetSwapInterval;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_makecurrent(IntPtr window, IntPtr context);
        public d_sdl_gl_makecurrent MakeCurrent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_gl_setattribute(Attribute attr, int value);
        private d_sdl_gl_setattribute SDL_GL_SetAttribute;

        public int SetAttribute(Attribute attr, int value)
        {
            int res = SDL_GL_SetAttribute(attr, value);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_setswapinterval(int interval);
        public d_sdl_gl_setswapinterval SetSwapInterval;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_swapwindow(IntPtr window);
        public d_sdl_gl_swapwindow SwapWindow;

        private void LoadEntryPoints(IntPtr library)
        {
            SDL_GL_CreateContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_createcontext>(library, "SDL_GL_CreateContext");
            DeleteContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_deletecontext>(library, "SDL_GL_DeleteContext");
            SDL_GL_GetCurrentContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getcurrentcontext>(library, "SDL_GL_GetCurrentContext");
            GetProcAddress = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getprocaddress>(library, "SDL_GL_GetProcAddress");
            GetSwapInterval = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getswapinterval>(library, "SDL_GL_GetSwapInterval");
            MakeCurrent = FuncLoader.LoadFunctionOrNull<d_sdl_gl_makecurrent>(library, "SDL_GL_MakeCurrent");
            SDL_GL_SetAttribute = FuncLoader.LoadFunctionOrNull<d_sdl_gl_setattribute>(library, "SDL_GL_SetAttribute");
            SetSwapInterval = FuncLoader.LoadFunctionOrNull<d_sdl_gl_setswapinterval>(library, "SDL_GL_SetSwapInterval");
            SwapWindow = FuncLoader.LoadFunctionOrNull<d_sdl_gl_swapwindow>(library, "SDL_GL_SwapWindow");
        }
    }
}
