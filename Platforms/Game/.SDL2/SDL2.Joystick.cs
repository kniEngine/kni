// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class Joystick
    {
        private Sdl _sdl;

        [Flags]
        public enum Hat : byte
        {
            Centered = 0,

            Up    = 1 << 0,
            Right = 1 << 1,
            Down  = 1 << 2,
            Left  = 1 << 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public uint TimeStamp;
            public int Which;
        }

        public Joystick(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_joystickclose(IntPtr joystick);
        public d_sdl_joystickclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private d_sdl_joystickfrominstanceid SDL_JoystickFromInstanceID;

        public IntPtr FromInstanceID(int joyid)
        {
            IntPtr pointer = SDL_JoystickFromInstanceID(joyid);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_joystickgetaxis(IntPtr joystick, int axis);
        public d_sdl_joystickgetaxis GetAxis;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_joystickgetbutton(IntPtr joystick, int button);
        public d_sdl_joystickgetbutton GetButton;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickname(IntPtr joystick);
        private d_sdl_joystickname JoystickName;

        public string GetJoystickName(IntPtr joystick)
        {
            return InteropHelpers.Utf8ToString(JoystickName(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Guid d_sdl_joystickgetguid(IntPtr joystick);
        public d_sdl_joystickgetguid GetGUID;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Hat d_sdl_joystickgethat(IntPtr joystick, int hat);
        public d_sdl_joystickgethat GetHat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickinstanceid(IntPtr joystick);
        public d_sdl_joystickinstanceid InstanceID;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickopen(int deviceIndex);
        private d_sdl_joystickopen SDL_JoystickOpen;

        public IntPtr Open(int deviceIndex)
        {
            IntPtr pointer = SDL_JoystickOpen(deviceIndex);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumaxes(IntPtr joystick);
        private d_sdl_joysticknumaxes SDL_JoystickNumAxes;

        public int NumAxes(IntPtr joystick)
        {
            int res = SDL_JoystickNumAxes(joystick);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumbuttons(IntPtr joystick);
        private d_sdl_joysticknumbuttons SDL_JoystickNumButtons;

        public int NumButtons(IntPtr joystick)
        {
            int res = SDL_JoystickNumButtons(joystick);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumhats(IntPtr joystick);
        private d_sdl_joysticknumhats SDL_JoystickNumHats;

        public int NumHats(IntPtr joystick)
        {
            int res = SDL_JoystickNumHats(joystick);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_numjoysticks();
        private d_sdl_numjoysticks SDL_NumJoysticks;

        public int NumJoysticks()
        {
            int res = SDL_NumJoysticks();
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_JoystickUpdate();
        public d_sdl_JoystickUpdate JoystickUpdate;

        private void LoadEntryPoints(IntPtr library)
        {
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_joystickclose>(library, "SDL_JoystickClose");
            SDL_JoystickFromInstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickfrominstanceid>(library, "SDL_JoystickFromInstanceID");
            GetAxis = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetaxis>(library, "SDL_JoystickGetAxis");
            GetButton = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetbutton>(library, "SDL_JoystickGetButton");
            JoystickName = FuncLoader.LoadFunctionOrNull<d_sdl_joystickname>(library, "SDL_JoystickName");
            GetGUID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetguid>(library, "SDL_JoystickGetGUID");
            GetHat = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgethat>(library, "SDL_JoystickGetHat");
            InstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickinstanceid>(library, "SDL_JoystickInstanceID");
            SDL_JoystickOpen = FuncLoader.LoadFunctionOrNull<d_sdl_joystickopen>(library, "SDL_JoystickOpen");
            SDL_JoystickNumAxes = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumaxes>(library, "SDL_JoystickNumAxes");
            SDL_JoystickNumButtons = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumbuttons>(library, "SDL_JoystickNumButtons");
            SDL_JoystickNumHats = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumhats>(library, "SDL_JoystickNumHats");
            SDL_NumJoysticks = FuncLoader.LoadFunctionOrNull<d_sdl_numjoysticks>(library, "SDL_NumJoysticks");
            JoystickUpdate = FuncLoader.LoadFunctionOrNull<d_sdl_JoystickUpdate>(library, "SDL_JoystickUpdate");
        }
    }
}
