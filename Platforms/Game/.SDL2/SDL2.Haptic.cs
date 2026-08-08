// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{    
    public class Haptic
    {
        private Sdl _sdl;

        // For some reason, different game controllers support different maximum values
        // Also, the closer a given res is to the maximum, the more likely the res will be ignored
        // Hence, we're setting an arbitrary safe res as a maximum
        public const uint Infinity = 1000000U;

        public enum EffectId : ushort
        {
            LeftRight = (1 << 2),
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LeftRight
        {
            public EffectId Type;
            public uint Length;
            public ushort LargeMagnitude;
            public ushort SmallMagnitude;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Effect
        {
            [FieldOffset(0)] public EffectId type;
            [FieldOffset(0)] public LeftRight leftright;
        }

        public Haptic(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_hapticclose(IntPtr haptic);
        public d_sdl_hapticclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_hapticeffectsupported(IntPtr haptic, ref Effect effect);
        public d_sdl_hapticeffectsupported EffectSupported;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickishaptic(IntPtr joystick);
        public d_sdl_joystickishaptic IsHaptic;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticneweffect(IntPtr haptic, ref Effect effect);
        private d_sdl_hapticneweffect SDL_HapticNewEffect;

        public void NewEffect(IntPtr haptic, ref Effect effect)
        {
            int res = SDL_HapticNewEffect(haptic, ref effect);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_hapticopen(int device_index);
        public d_sdl_hapticopen Open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_hapticopenfromjoystick(IntPtr joystick);
        private d_sdl_hapticopenfromjoystick SDL_HapticOpenFromJoystick;

        public IntPtr OpenFromJoystick(IntPtr joystick)
        {
            IntPtr pointer = SDL_HapticOpenFromJoystick(joystick);
            _sdl.GetError(pointer);
            return pointer;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleinit(IntPtr haptic);
        private d_sdl_hapticrumbleinit SDL_HapticRumbleInit;

        public void RumbleInit(IntPtr haptic)
        {
            int res = SDL_HapticRumbleInit(haptic);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleplay(IntPtr haptic, float strength, uint length);
        private d_sdl_hapticrumbleplay SDL_HapticRumblePlay;

        public void RumblePlay(IntPtr haptic, float strength, uint length)
        {
            int res = SDL_HapticRumblePlay(haptic, strength, length);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumblesupported(IntPtr haptic);
        private d_sdl_hapticrumblesupported SDL_HapticRumbleSupported;

        public int RumbleSupported(IntPtr haptic)
        {
            int res = SDL_HapticRumbleSupported(haptic);
            _sdl.GetError(res);
            return res;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticruneffect(IntPtr haptic, int effect, uint iterations);
        private d_sdl_hapticruneffect SDL_HapticRunEffect;

        public void RunEffect(IntPtr haptic, int effect, uint iterations)
        {
            int res = SDL_HapticRunEffect(haptic, effect, iterations);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticstopall(IntPtr haptic);
        private d_sdl_hapticstopall SDL_HapticStopAll;

        public void StopAll(IntPtr haptic)
        {
            int res = SDL_HapticStopAll(haptic);
            _sdl.GetError(res);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticupdateeffect(IntPtr haptic, int effect, ref Effect data);
        private d_sdl_hapticupdateeffect SDL_HapticUpdateEffect;

        public void UpdateEffect(IntPtr haptic, int effect, ref Effect data)
        {
            int res = SDL_HapticUpdateEffect(haptic, effect, ref data);
            _sdl.GetError(res);
        }

        private void LoadEntryPoints(IntPtr library)
        {
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_hapticclose>(library, "SDL_HapticClose");
            EffectSupported = FuncLoader.LoadFunctionOrNull<d_sdl_hapticeffectsupported>(library, "SDL_HapticEffectSupported");
            IsHaptic = FuncLoader.LoadFunctionOrNull<d_sdl_joystickishaptic>(library, "SDL_JoystickIsHaptic");
            SDL_HapticNewEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticneweffect>(library, "SDL_HapticNewEffect");
            Open = FuncLoader.LoadFunctionOrNull<d_sdl_hapticopen>(library, "SDL_HapticOpen");
            SDL_HapticOpenFromJoystick = FuncLoader.LoadFunctionOrNull<d_sdl_hapticopenfromjoystick>(library, "SDL_HapticOpenFromJoystick");
            SDL_HapticRumbleInit = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumbleinit>(library, "SDL_HapticRumbleInit");
            SDL_HapticRumblePlay = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumbleplay>(library, "SDL_HapticRumblePlay");
            SDL_HapticRumbleSupported = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumblesupported>(library, "SDL_HapticRumbleSupported");
            SDL_HapticRunEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticruneffect>(library, "SDL_HapticRunEffect");
            SDL_HapticStopAll = FuncLoader.LoadFunctionOrNull<d_sdl_hapticstopall>(library, "SDL_HapticStopAll");
            SDL_HapticUpdateEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticupdateeffect>(library, "SDL_HapticUpdateEffect");
        }
    }
}
