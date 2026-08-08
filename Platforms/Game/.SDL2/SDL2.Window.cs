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
    public class Window
    {
        private Sdl _sdl;

        public const int PosUndefined = 0x1FFF0000;
        public const int PosCentered = 0x2FFF0000;

        public enum EventId : byte
        {
            None,
            Shown,
            Hidden,
            Exposed,
            Moved,
            Resized,
            SizeChanged,
            Minimized,
            Maximized,
            Restored,
            Enter,
            Leave,
            FocusGained,
            FocusLost,
            Close,
        }


        public Window(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        public enum State : int
        {
            Fullscreen      = 0x00000001,
            OpenGL          = 0x00000002,
            Shown           = 0x00000004,
            Hidden          = 0x00000008,
            Borderless      = 0x00000010,
            Resizable       = 0x00000020,
            Minimized       = 0x00000040,
            Maximized       = 0x00000080,
            Grabbed         = 0x00000100,
            InputFocus      = 0x00000200,
            MouseFocus      = 0x00000400,
            Foreign         = 0x00000800,
            FullscreenDesktop = 0x00001001,
            AllowHighDPI    = 0x00002000,
            MouseCapture    = 0x00004000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public uint TimeStamp;
            public uint WindowID;
            public EventId EventID;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int Data1;
            public int Data2;
        }

        public enum SysWMType
        {
            Unknown,
            Windows,
            X11,
            Directfb,
            Cocoa,
            UiKit,
            Wayland,
            Mir,
            WinRt,
            Android
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_SysWMinfo_Win
        {
            public IntPtr window; // HWND
            public IntPtr hdc;    // HDC
            public IntPtr hinstance; // HINSTANCE
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct SDL_SysWMinfo_Info
        {
            [FieldOffset(0)]
            public SDL_SysWMinfo_Win win;
            // additional platforms here...
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_SysWMinfo
        {
            public Version version;
            public SysWMType subsystem;
            public SDL_SysWMinfo_Info info;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createwindow(string title, int x, int y, int w, int h, int flags);
        private d_sdl_createwindow SDL_CreateWindow;

        public IntPtr Create(string title, int x, int y, int w, int h, Sdl.Window.State flags)
        {
            IntPtr pointer = SDL_CreateWindow(title, x, y, w, h, (int)flags);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_destroywindow(IntPtr window);
        public d_sdl_destroywindow Destroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint d_sdl_getwindowid(IntPtr window);
        public d_sdl_getwindowid GetWindowId;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex;

        public int GetDisplayIndex(IntPtr window)
        {
            int res = SDL_GetWindowDisplayIndex(window);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowflags(IntPtr window);
        public d_sdl_getwindowflags GetWindowFlags;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowicon(IntPtr window, IntPtr icon);
        public d_sdl_setwindowicon SetIcon;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowposition(IntPtr window, out int x, out int y);
        public d_sdl_getwindowposition GetPosition;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowsize(IntPtr window, out int w, out int h);
        public d_sdl_getwindowsize GetSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowbordered(IntPtr window, int bordered);
        public d_sdl_setwindowbordered SetBordered;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_setwindowfullscreen(IntPtr window, int flags);
        private d_sdl_setwindowfullscreen SDL_SetWindowFullscreen;

        public void SetFullscreen(IntPtr window, Sdl.Window.State flags)
        {
            int res = SDL_SetWindowFullscreen(window, (int)flags);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowposition(IntPtr window, int x, int y);
        public d_sdl_setwindowposition SetPosition;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowresizable(IntPtr window, bool resizable);
        public d_sdl_setwindowresizable SetResizable;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowsize(IntPtr window, int w, int h);
        public d_sdl_setwindowsize SetSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void d_sdl_setwindowtitle(IntPtr window, byte* value);
        private d_sdl_setwindowtitle SDL_SetWindowTitle;

        public unsafe void SetTitle(IntPtr handle, string title)
        {
            byte[] str = Encoding.UTF8.GetBytes(title+'\0');

            fixed (byte* pStr = str)
            {
                SDL_SetWindowTitle(handle, pStr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_showwindow(IntPtr window);
        public d_sdl_showwindow Show;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_sdl_getwindowwminfo(IntPtr window, ref SDL_SysWMinfo sysWMinfo);
        public d_sdl_getwindowwminfo GetWindowWMInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowborderssize(IntPtr window, out int top, out int left, out int right, out int bottom);
        public d_sdl_getwindowborderssize GetBorderSize;


        private void LoadEntryPoints(IntPtr library)
        {
            SDL_CreateWindow = FuncLoader.LoadFunctionOrNull<d_sdl_createwindow>(library, "SDL_CreateWindow");
            Destroy = FuncLoader.LoadFunctionOrNull<d_sdl_destroywindow>(library, "SDL_DestroyWindow");
            GetWindowId = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowid>(library, "SDL_GetWindowID");
            SDL_GetWindowDisplayIndex = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowdisplayindex>(library, "SDL_GetWindowDisplayIndex");
            GetWindowFlags = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowflags>(library, "SDL_GetWindowFlags");
            SetIcon = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowicon>(library, "SDL_SetWindowIcon");
            GetPosition = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowposition>(library, "SDL_GetWindowPosition");
            GetSize = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowsize>(library, "SDL_GetWindowSize");
            SetBordered = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowbordered>(library, "SDL_SetWindowBordered");
            SDL_SetWindowFullscreen = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowfullscreen>(library, "SDL_SetWindowFullscreen");
            SetPosition = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowposition>(library, "SDL_SetWindowPosition");
            SetResizable = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowresizable>(library, "SDL_SetWindowResizable");
            SetSize = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowsize>(library, "SDL_SetWindowSize");
            SDL_SetWindowTitle = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowtitle>(library, "SDL_SetWindowTitle");
            Show = FuncLoader.LoadFunctionOrNull<d_sdl_showwindow>(library, "SDL_ShowWindow");
            GetWindowWMInfo = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowwminfo>(library, "SDL_GetWindowWMInfo");
            GetBorderSize = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowborderssize>(library, "SDL_GetWindowBordersSize");
        }
    }
}
