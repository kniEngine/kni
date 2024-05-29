// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteJoystick : JoystickStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        // map Joystick indices (PlayerIndex) -> JoystickDevices
        private Dictionary<int, SdlJoystickDevice> _sdlJoysticks = new Dictionary<int, SdlJoystickDevice>();
        // map Joystick instanceIDs -> gamepad indices (PlayerIndex)
        private readonly Dictionary<int, int> _indicesMap = new Dictionary<int, int>();

        private int _maxConnectedIndex = -1;

        public override bool PlatformIsSupported
        {
            get { return true; }
        }

        public override int PlatformLastConnectedIndex
        {
            get { return _maxConnectedIndex; }
        }

        public ConcreteJoystick()
        {

        }

        ~ConcreteJoystick()
        {
            foreach (SdlJoystickDevice sdlJoystick in _sdlJoysticks.Values)
                SDL.JOYSTICK.Close(sdlJoystick.Handle);

            _sdlJoysticks.Clear();
            _indicesMap.Clear();
        }

        public override JoystickCapabilities PlatformGetCapabilities(int index)
        {
            if (_sdlJoysticks.TryGetValue(index, out SdlJoystickDevice sdlJoystick))
                return sdlJoystick.Capabilities;

            return base.CreateJoystickCapabilities(false, string.Empty, string.Empty, false, 0, 0, 0);
        }

        public override JoystickState PlatformGetState(int index)
        {
            if (_sdlJoysticks.TryGetValue(index, out SdlJoystickDevice sdlJoystick))
            {
                JoystickCapabilities caps = sdlJoystick.Capabilities;

                int[] axes = new int[caps.AxisCount];
                for (int i = 0; i < axes.Length; i++)
                    axes[i] = SDL.JOYSTICK.GetAxis(sdlJoystick.Handle, i);

                ButtonState[] buttons = new ButtonState[caps.ButtonCount];
                for (int i = 0; i < buttons.Length; i++)
                    buttons[i] = (SDL.JOYSTICK.GetButton(sdlJoystick.Handle, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

                JoystickHat[] hats = new JoystickHat[caps.HatCount];
                for (int i = 0; i < hats.Length; i++)
                {
                    Sdl.Joystick.Hat hatstate = SDL.JOYSTICK.GetHat(sdlJoystick.Handle, i);
                    Buttons dPadButtons = SDLToXnaDPadButtons(hatstate);

                    hats[i] = base.CreateJoystickHat(dPadButtons);
                }

                sdlJoystick.State = base.CreateJoystickState(
                                            isConnected: true,
                                            axes: axes,
                                            buttons: buttons,
                                            hats: hats
                                        );

                return sdlJoystick.State;
            }

            return JoystickStrategy.DefaultJoystickState;

        }

        public override void PlatformGetState(int index, ref JoystickState joystickState)
        {
            bool isConnected = false;
            int[] axes = joystickState.Axes;
            ButtonState[] buttons = joystickState.Buttons;
            JoystickHat[] hats = joystickState.Hats;

            if (_sdlJoysticks.TryGetValue(index, out SdlJoystickDevice sdlJoystick))
            {
                JoystickCapabilities caps = sdlJoystick.Capabilities;

                //Resize each array if the length is less than the count returned by the capabilities
                if (axes.Length < caps.AxisCount)
                    axes = new int[caps.AxisCount];

                if (buttons.Length < caps.ButtonCount)
                    buttons = new ButtonState[caps.ButtonCount];

                if (hats.Length < caps.HatCount)
                    hats = new JoystickHat[caps.HatCount];

                for (int i = 0; i < caps.AxisCount; i++)
                    axes[i] = SDL.JOYSTICK.GetAxis(sdlJoystick.Handle, i);

                for (int i = 0; i < caps.ButtonCount; i++)
                    buttons[i] = (SDL.JOYSTICK.GetButton(sdlJoystick.Handle, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

                for (int i = 0; i < caps.HatCount; i++)
                {
                    Sdl.Joystick.Hat hatstate = SDL.JOYSTICK.GetHat(sdlJoystick.Handle, i);
                    Buttons dPadButtons = SDLToXnaDPadButtons(hatstate);

                    hats[i] = base.CreateJoystickHat(dPadButtons);
                }

                isConnected = true;
            }

            joystickState = base.CreateJoystickState(isConnected, axes, buttons, hats);
        }


        internal void AddDevices()
        {
            int numJoysticks = SDL.JOYSTICK.NumJoysticks();
            for (int deviceIndex = 0; deviceIndex < numJoysticks; deviceIndex++)
                AddDevice(deviceIndex);
        }

        internal void AddDevice(int deviceIndex)
        {
            IntPtr handle = SDL.JOYSTICK.Open(deviceIndex);
            foreach (SdlJoystickDevice joystickDevice in _sdlJoysticks.Values)
                if (joystickDevice.Handle == handle)
                    return;

            int index = 0;
            while (_sdlJoysticks.ContainsKey(index))
                index++;

            _maxConnectedIndex = Math.Max(_maxConnectedIndex, index);

            int instanceID = SDL.JOYSTICK.InstanceID(handle);
            SdlJoystickDevice sdlJoystick = new SdlJoystickDevice(handle);
            sdlJoystick.Capabilities = base.CreateJoystickCapabilities(
                    isConnected: true,
                    displayName: SDL.JOYSTICK.GetJoystickName(handle),
                    identifier:  SDL.JOYSTICK.GetGUID(handle).ToString(),
                    isGamepad:   (SDL.GAMECONTROLLER.IsGameController(deviceIndex) == 1),
                    axisCount:   SDL.JOYSTICK.NumAxes(handle),
                    buttonCount: SDL.JOYSTICK.NumButtons(handle),
                    hatCount:    SDL.JOYSTICK.NumHats(handle)
                );

            _sdlJoysticks.Add(index, sdlJoystick);

            _indicesMap[instanceID] = index;

            if (sdlJoystick.Capabilities.IsGamepad)
                ((IPlatformGamePad)GamePad.Current).GetStrategy<ConcreteGamePad>().AddDevice(deviceIndex);
        }

        internal void RemoveDevice(int instanceID)
        {
            if (_indicesMap.TryGetValue(instanceID, out int index))
            {
                if (_sdlJoysticks.TryGetValue(index, out SdlJoystickDevice sdlJoystick))
                {
                    SDL.JOYSTICK.Close(sdlJoystick.Handle);
                    _sdlJoysticks.Remove(index);

                    if (_maxConnectedIndex == index)
                        _maxConnectedIndex = CalculateMaxConnectedIndex();

                    _indicesMap.Remove(instanceID);
                }
            }
        }

        private int CalculateMaxConnectedIndex()
        {
            int maxConnectedIndex = -1;
            foreach (int index in _sdlJoysticks.Keys)
                maxConnectedIndex = Math.Max(maxConnectedIndex, index);

            return maxConnectedIndex;
        }

        private Buttons SDLToXnaDPadButtons(Sdl.Joystick.Hat hatstate)
        {
            Buttons dPadButtons = (Buttons)0;
            dPadButtons |= (Buttons)((int)(hatstate & Sdl.Joystick.Hat.Up));
            dPadButtons |= (Buttons)((int)(hatstate & Sdl.Joystick.Hat.Down) >> 1);
            dPadButtons |= (Buttons)((int)(hatstate & Sdl.Joystick.Hat.Left) >> 1);
            dPadButtons |= (Buttons)((int)(hatstate & Sdl.Joystick.Hat.Right) << 2);

            return dPadButtons;
        }

    }

    public class SdlJoystickDevice : JoystickDevice
    {

        public IntPtr Handle { get; private set; }

        internal JoystickState State;


        public SdlJoystickDevice(IntPtr handle)
            : base()
        {
            this.Handle = handle;
        }
    }

    public class JoystickDevice
    {
        public JoystickCapabilities Capabilities;

        public JoystickDevice()
        {
        }
    }
}

