// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    public class GameController
    {
        private Sdl _sdl;

        public enum Axis
        {
            Invalid = -1,
            LeftX,
            LeftY,
            RightX,
            RightY,
            TriggerLeft,
            TriggerRight,
            Max,
        }

        public enum Button
        {
            Invalid = -1,
            A,
            B,
            X,
            Y,
            Back,
            Guide,
            Start,
            LeftStick,
            RightStick,
            LeftShoulder,
            RightShoulder,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight,
            Max,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public uint TimeStamp;
            public int Which;
        }

        public GameController(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmapping(string mappingString);
        public d_sdl_gamecontrolleraddmapping AddMapping;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmappingsfromrw(IntPtr rw, int freew);
        public d_sdl_gamecontrolleraddmappingsfromrw AddMappingFromRw;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gamecontrollerclose(IntPtr gamecontroller);
        public d_sdl_gamecontrollerclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private d_sdl_joystickfrominstanceid SDL_GameControllerFromInstanceID;

        public IntPtr FromInstanceID(int joyid)
        {
            IntPtr pointer = SDL_GameControllerFromInstanceID(joyid);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_gamecontrollergetaxis(IntPtr gamecontroller, Axis axis);
        public d_sdl_gamecontrollergetaxis GetAxis;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollergetbutton(IntPtr gamecontroller, Button button);
        public d_sdl_gamecontrollergetbutton GetButton;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollergetjoystick(IntPtr gamecontroller);
        private d_sdl_gamecontrollergetjoystick SDL_GameControllerGetJoystick;

        public IntPtr GetJoystick(IntPtr gamecontroller)
        {
            IntPtr pointer = SDL_GameControllerGetJoystick(gamecontroller);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_isgamecontroller(int joystickIndex);
        public d_sdl_isgamecontroller IsGameController;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_gamecontrollermapping(IntPtr gamecontroller);
        public d_sdl_gamecontrollermapping SDL_GameControllerMapping;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrolleropen(int joystickIndex);
        private d_sdl_gamecontrolleropen SDL_GameControllerOpen;

        public IntPtr Open(int joystickIndex)
        {
            IntPtr pointer = SDL_GameControllerOpen(joystickIndex);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollername(IntPtr gamecontroller);
        private d_sdl_gamecontrollername SDL_GameControllerName;

        public string GetName(IntPtr gamecontroller)
        {
            return InteropHelpers.Utf8ToString(SDL_GameControllerName(gamecontroller));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrollerrumble(IntPtr gamecontroller, ushort left, ushort right, uint duration);
        public d_sdl_gamecontrollerrumble Rumble;
        public d_sdl_gamecontrollerrumble RumbleTriggers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollerhasrumble(IntPtr gamecontroller);
        public d_sdl_gamecontrollerhasrumble HasRumble;
        public d_sdl_gamecontrollerhasrumble HasRumbleTriggers;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_GameControllerUpdate();
        public d_sdl_GameControllerUpdate GameControllerUpdate;

        private void LoadEntryPoints(IntPtr library)
        {            
            AddMapping = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleraddmapping>(library, "SDL_GameControllerAddMapping");
            AddMappingFromRw = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleraddmappingsfromrw>(library, "SDL_GameControllerAddMappingsFromRW");
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerclose>(library, "SDL_GameControllerClose");
            SDL_GameControllerFromInstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickfrominstanceid>(library, "SDL_JoystickFromInstanceID");
            GetAxis = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetaxis>(library, "SDL_GameControllerGetAxis");
            GetButton = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetbutton>(library, "SDL_GameControllerGetButton");
            SDL_GameControllerGetJoystick = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetjoystick>(library, "SDL_GameControllerGetJoystick");
            IsGameController = FuncLoader.LoadFunctionOrNull<d_sdl_isgamecontroller>(library, "SDL_IsGameController");
            SDL_GameControllerMapping = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollermapping>(library, "SDL_GameControllerMapping");
            SDL_GameControllerOpen = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleropen>(library, "SDL_GameControllerOpen");
            SDL_GameControllerName = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollername>(library, "SDL_GameControllerName");
            Rumble = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerrumble>(library, "SDL_GameControllerRumble");
            RumbleTriggers = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerrumble>(library, "SDL_GameControllerRumbleTriggers");
            HasRumble = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerhasrumble>(library, "SDL_GameControllerHasRumble");
            HasRumbleTriggers = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerhasrumble>(library, "SDL_GameControllerHasRumbleTriggers");
            GameControllerUpdate = FuncLoader.LoadFunctionOrNull<d_sdl_GameControllerUpdate>(library, "SDL_GameControllerUpdate");
        }
    }
}
