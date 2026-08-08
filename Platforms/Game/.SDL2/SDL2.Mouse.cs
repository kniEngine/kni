// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class Mouse
    {
        private Sdl _sdl;

        [Flags]
        public enum Button
        {
            Left = 1 << 0,
            Middle = 1 << 1,
            Right = 1 << 2,
            X1Mask = 1 << 3,
            X2Mask = 1 << 4
        }

        public enum SystemCursor
        {
            Arrow,
            IBeam,
            Wait,
            Crosshair,
            WaitArrow,
            SizeNWSE,
            SizeNESW,
            SizeWE,
            SizeNS,
            SizeAll,
            No,
            Hand
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MotionEvent
        {
            public uint Timestamp;
            public uint WindowID;
            public uint Which;
            public byte State;
            private byte _padding1;
            private byte _padding2;
            private byte _padding3;
            public int X;
            public int Y;
            public int Xrel;
            public int Yrel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelEvent
        {
            public uint TimeStamp;
            public uint WindowId;
            public uint Which;
            public int X;
            public int Y;
            public uint Direction;
        }

        public Mouse(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createcolorcursor(IntPtr surface, int x, int y);
        private d_sdl_createcolorcursor SDL_CreateColorCursor;

        public IntPtr CreateColorCursor(IntPtr surface, int x, int y)
        {
            IntPtr pointer = SDL_CreateColorCursor(surface, x, y);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createsystemcursor(SystemCursor id);
        private d_sdl_createsystemcursor SDL_CreateSystemCursor;

        public IntPtr CreateSystemCursor(SystemCursor id)
        {
            IntPtr pointer = SDL_CreateSystemCursor(id);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_freecursor(IntPtr cursor);
        public d_sdl_freecursor FreeCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getglobalmousestate(out int x, out int y);
        public d_sdl_getglobalmousestate GetGlobalState;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getmousestate(out int x, out int y);
        public d_sdl_getmousestate GetState;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getrelativemousestate(out int x, out int y);
        public d_sdl_getrelativemousestate GetRelativeState;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setcursor(IntPtr cursor);
        public d_sdl_setcursor SetCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_showcursor(int toggle);
        public d_sdl_showcursor ShowCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_warpmouseinwindow(IntPtr window, int x, int y);
        public d_sdl_warpmouseinwindow WarpInWindow;

        private void LoadEntryPoints(IntPtr library)
        {
            SDL_CreateColorCursor = FuncLoader.LoadFunctionOrNull<d_sdl_createcolorcursor>(library, "SDL_CreateColorCursor");
            SDL_CreateSystemCursor = FuncLoader.LoadFunctionOrNull<d_sdl_createsystemcursor>(library, "SDL_CreateSystemCursor");
            FreeCursor = FuncLoader.LoadFunctionOrNull<d_sdl_freecursor>(library, "SDL_FreeCursor");
            GetGlobalState = FuncLoader.LoadFunctionOrNull<d_sdl_getglobalmousestate>(library, "SDL_GetGlobalMouseState");
            GetState = FuncLoader.LoadFunctionOrNull<d_sdl_getmousestate>(library, "SDL_GetMouseState");
            GetRelativeState = FuncLoader.LoadFunctionOrNull<d_sdl_getrelativemousestate>(library, "SDL_GetRelativeMouseState");
            SetCursor = FuncLoader.LoadFunctionOrNull<d_sdl_setcursor>(library, "SDL_SetCursor");
            ShowCursor = FuncLoader.LoadFunctionOrNull<d_sdl_showcursor>(library, "SDL_ShowCursor");
            WarpInWindow = FuncLoader.LoadFunctionOrNull<d_sdl_warpmouseinwindow>(library, "SDL_WarpMouseInWindow");
        }
    }
}
