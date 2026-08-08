// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class Display
    {
        private Sdl _sdl;

        public struct Mode
        {
            public uint Format;
            public int Width;
            public int Height;
            public int RefreshRate;
            public IntPtr DriverData;
        }

        public Display(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaybounds(int displayIndex, out Rectangle rect);
        private d_sdl_getdisplaybounds SDL_GetDisplayBounds;

        public void GetBounds(int displayIndex, out Rectangle rect)
        {
            int res = SDL_GetDisplayBounds(displayIndex, out rect);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getcurrentdisplaymode(int displayIndex, out Mode mode);
        private d_sdl_getcurrentdisplaymode SDL_GetCurrentDisplayMode;

        public void GetCurrentDisplayMode(int displayIndex, out Mode mode)
        {
            int res = SDL_GetCurrentDisplayMode(displayIndex, out mode);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaymode(int displayIndex, int modeIndex, out Mode mode);
        private d_sdl_getdisplaymode SDL_GetDisplayMode;

        public void GetDisplayMode(int displayIndex, int modeIndex, out Mode mode)
        {
            int res = SDL_GetDisplayMode(displayIndex, modeIndex, out mode);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getclosestdisplaymode(int displayIndex, Mode mode, out Mode closest);
        private d_sdl_getclosestdisplaymode SDL_GetClosestDisplayMode;

        public void GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest)
        {
            int res = SDL_GetClosestDisplayMode(displayIndex, mode, out closest);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_getdisplayname(int index);
        private d_sdl_getdisplayname SDL_GetDisplayName;

        public string GetDisplayName(int index)
        {
            IntPtr pointer = SDL_GetDisplayName(index);
            _sdl.GetError(pointer);
            return InteropHelpers.Utf8ToString(pointer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumdisplaymodes(int displayIndex);
        private d_sdl_getnumdisplaymodes SDL_GetNumDisplayModes;

        public int GetNumDisplayModes(int displayIndex)
        {
            int res = SDL_GetNumDisplayModes(displayIndex);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumvideodisplays();
        private d_sdl_getnumvideodisplays SDL_GetNumVideoDisplays;

        public int GetNumVideoDisplays()
        {
            int res = SDL_GetNumVideoDisplays();
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex;

        public int GetWindowDisplayIndex(IntPtr window)
        {
            int res = SDL_GetWindowDisplayIndex(window);
            _sdl.GetError(res);
            return res;
        }


        private void LoadEntryPoints(IntPtr library)
        {
            SDL_GetDisplayBounds = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplaybounds>(library, "SDL_GetDisplayBounds");
            SDL_GetCurrentDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getcurrentdisplaymode>(library, "SDL_GetCurrentDisplayMode");
            SDL_GetDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplaymode>(library, "SDL_GetDisplayMode");
            SDL_GetClosestDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getclosestdisplaymode>(library, "SDL_GetClosestDisplayMode");
            SDL_GetDisplayName = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplayname>(library, "SDL_GetDisplayName");
            SDL_GetNumDisplayModes = FuncLoader.LoadFunctionOrNull<d_sdl_getnumdisplaymodes>(library, "SDL_GetNumDisplayModes");
            SDL_GetNumVideoDisplays = FuncLoader.LoadFunctionOrNull<d_sdl_getnumvideodisplays>(library, "SDL_GetNumVideoDisplays");
            SDL_GetWindowDisplayIndex = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowdisplayindex>(library, "SDL_GetWindowDisplayIndex");
        }
    }
}
