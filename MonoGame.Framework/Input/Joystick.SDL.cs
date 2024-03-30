// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    sealed partial class Joystick
    {
        private Sdl SDL { get { return Sdl.Current; } }

        internal Dictionary<int, IntPtr> Joysticks = new Dictionary<int, IntPtr>();
        private int _lastConnectedIndex = -1;

        private bool PlatformIsSupported
        {
            get { return true; }
        }

        private int PlatformLastConnectedIndex
        {
            get { return _lastConnectedIndex; }
        }


        private JoystickCapabilities PlatformGetCapabilities(int index)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
                return new JoystickCapabilities
                {
                    IsConnected = false,
                    DisplayName = string.Empty,
                    Identifier = "",
                    IsGamepad = false,
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            IntPtr jdevice = joystickPtr;
            return new JoystickCapabilities
            {
                IsConnected = true,
                DisplayName = SDL.JOYSTICK.GetJoystickName(jdevice),
                Identifier = SDL.JOYSTICK.GetGUID(jdevice).ToString(),
                IsGamepad = (SDL.GAMECONTROLLER.IsGameController(index) == 1),
                AxisCount = SDL.JOYSTICK.NumAxes(jdevice),
                ButtonCount = SDL.JOYSTICK.NumButtons(jdevice), 
                HatCount = SDL.JOYSTICK.NumHats(jdevice)
            };
        }

        private JoystickState PlatformGetState(int index)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
                return Joystick.DefaultJoystickState;

            JoystickCapabilities jcap = PlatformGetCapabilities(index);
            IntPtr jdevice = joystickPtr;

            int[] axes = new int[jcap.AxisCount];
            for (int i = 0; i < axes.Length; i++)
                axes[i] = SDL.JOYSTICK.GetAxis(jdevice, i);

            ButtonState[] buttons = new ButtonState[jcap.ButtonCount];
            for (int i = 0; i < buttons.Length; i++)
                buttons[i] = (SDL.JOYSTICK.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            JoystickHat[] hats = new JoystickHat[jcap.HatCount];
            for (int i = 0; i < hats.Length; i++)
            {
                Sdl.Joystick.Hat hatstate = SDL.JOYSTICK.GetHat(jdevice, i);

                hats[i] = new JoystickHat
                {
                    Up   = ((hatstate & Sdl.Joystick.Hat.Up) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Down = ((hatstate & Sdl.Joystick.Hat.Down) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Left  = ((hatstate & Sdl.Joystick.Hat.Left) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Right = ((hatstate & Sdl.Joystick.Hat.Right) != 0) ? ButtonState.Pressed : ButtonState.Released
                };
            }

            return new JoystickState
            {
                IsConnected = true,
                Axes = axes,
                Buttons = buttons,
                Hats = hats
            };
        }

        private void PlatformGetState(int index, ref JoystickState joystickState)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
            {
                joystickState.IsConnected = false;
                return;
            }

            JoystickCapabilities jcap = PlatformGetCapabilities(index);
            IntPtr jdevice = joystickPtr;

            //Resize each array if the length is less than the count returned by the capabilities
            if (joystickState.Axes.Length < jcap.AxisCount)
            {
                joystickState.Axes = new int[jcap.AxisCount];
            }

            if (joystickState.Buttons.Length < jcap.ButtonCount)
            {
                joystickState.Buttons = new ButtonState[jcap.ButtonCount];
            }

            if (joystickState.Hats.Length < jcap.HatCount)
            {
                joystickState.Hats = new JoystickHat[jcap.HatCount];
            }

            for (int i = 0; i < jcap.AxisCount; i++)
                joystickState.Axes[i] = SDL.JOYSTICK.GetAxis(jdevice, i);

            for (int i = 0; i < jcap.ButtonCount; i++)
                joystickState.Buttons[i] = (SDL.JOYSTICK.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            for (int i = 0; i < jcap.HatCount; i++)
            {
                Sdl.Joystick.Hat hatstate = SDL.JOYSTICK.GetHat(jdevice, i);

                joystickState.Hats[i] = new JoystickHat
                {
                    Up   = ((hatstate & Sdl.Joystick.Hat.Up) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Down = ((hatstate & Sdl.Joystick.Hat.Down) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Left  = ((hatstate & Sdl.Joystick.Hat.Left) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Right = ((hatstate & Sdl.Joystick.Hat.Right) != 0) ? ButtonState.Pressed : ButtonState.Released
                };
            }

            joystickState.IsConnected = true;
        }


        internal void AddDevices()
        {
            int numJoysticks = SDL.JOYSTICK.NumJoysticks();
            for (int i = 0; i < numJoysticks; i++)
                AddDevice(i);
        }

        internal void AddDevice(int deviceId)
        {
            IntPtr jdevice = SDL.JOYSTICK.Open(deviceId);
            if (Joysticks.ContainsValue(jdevice)) return;

            int id = 0;

            while (Joysticks.ContainsKey(id))
                id++;

            if (id > _lastConnectedIndex)
                _lastConnectedIndex = id;

            Joysticks.Add(id, jdevice);

            if (SDL.GAMECONTROLLER.IsGameController(deviceId) == 1)
                GamePad.Current.AddDevice(deviceId);
        }

        internal void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, IntPtr> entry in Joysticks)
            {
                if (SDL.JOYSTICK.InstanceID(entry.Value) == instanceid)
                {
                    int key = entry.Key;

                    SDL.JOYSTICK.Close(Joysticks[entry.Key]);
                    Joysticks.Remove(entry.Key);

                    if (key == _lastConnectedIndex)
                        RecalculateLastConnectedIndex();

                    break;
                }
            }
        }

        internal void CloseDevices()
        {
            GamePad.Current.CloseDevices();

            foreach (KeyValuePair<int,IntPtr> entry in Joysticks)
                SDL.JOYSTICK.Close(entry.Value);

            Joysticks.Clear();
        }

        private void RecalculateLastConnectedIndex()
        {
            _lastConnectedIndex = -1;
            foreach (KeyValuePair<int, IntPtr> entry in Joysticks)
            {
                if (entry.Key > _lastConnectedIndex)
                    _lastConnectedIndex = entry.Key;
            }
        }


    }
}

