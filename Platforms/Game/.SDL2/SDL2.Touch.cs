// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class Touch
    {
        private Sdl _sdl;

        public enum TouchDeviceType
        {
            Invalid          =   -1,
            Direct           = 0x00,
            IndirectAbsolute = 0x01,
            IndirectRelative = 0x02,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FingerEvent
        {
            public uint Timestamp;
            public long TouchId;
            public long FingerId;
            public float X;
            public float Y;
            public float Dx;
            public float Dy;
            public float Pressure;
            public uint WindowID;
        }

        public Touch(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getnumtouchdevices();
        public d_sdl_getnumtouchdevices GetNumTouchDevices;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long d_sdl_gettouchdevice(int index);
        public d_sdl_gettouchdevice GetTouchDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getnumtouchfingers(long touchId);
        public d_sdl_getnumtouchfingers GetNumTouchFingers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate TouchDeviceType d_sdl_gettouchdevicetype(long touchId);
        public d_sdl_gettouchdevicetype GetTouchDeviceType;

        private void LoadEntryPoints(IntPtr library)
        {
            GetNumTouchDevices = FuncLoader.LoadFunctionOrNull<d_sdl_getnumtouchdevices>(library, "SDL_GetNumTouchDevices");
            GetTouchDevice = FuncLoader.LoadFunctionOrNull<d_sdl_gettouchdevice>(library, "SDL_GetTouchDevice");
            GetNumTouchFingers = FuncLoader.LoadFunctionOrNull<d_sdl_getnumtouchfingers>(library, "SDL_GetNumTouchFingers");
            GetTouchDeviceType = FuncLoader.LoadFunctionOrNull<d_sdl_gettouchdevicetype>(library, "SDL_GetTouchDeviceType");
        }
    }
}
