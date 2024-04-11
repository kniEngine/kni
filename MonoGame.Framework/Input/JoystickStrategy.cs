// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class JoystickStrategy
    {
        public abstract bool PlatformIsSupported { get; }
        public abstract int PlatformLastConnectedIndex { get; }

        public abstract JoystickCapabilities PlatformGetCapabilities(int index);
        public abstract JoystickState PlatformGetState(int index);
        public abstract void PlatformGetState(int index, ref JoystickState joystickState);

        protected JoystickCapabilities CreateJoystickCapabilities(
            bool isConnected, string displayName, string identifier,
            bool isGamepad, int axisCount, int buttonCount, int hatCount)
        {
            JoystickCapabilities caps = new JoystickCapabilities();
            caps.IsConnected = isConnected;
            caps.DisplayName = displayName;
            caps.Identifier = identifier;
            caps.IsGamepad = isGamepad;
            caps.AxisCount = axisCount;
            caps.ButtonCount = buttonCount;
            caps.HatCount = hatCount;

            return caps;
        }

        protected JoystickState CreateJoystickState(bool isConnected, int[] axes, ButtonState[] buttons, JoystickHat[] hats)
        {
            JoystickState state = new JoystickState();
            state.IsConnected = isConnected;
            state.Axes = axes;
            state.Buttons = buttons;
            state.Hats = hats;

            return state;
        }

        protected JoystickHat CreateJoystickHat(Buttons dPadButtons)
        {
            return new JoystickHat(dPadButtons);
        }

        /// <summary>
        /// A default <see cref="JoystickState"/>.
        /// </summary>
        protected static JoystickState DefaultJoystickState = new JoystickState
        {
            IsConnected = false,
            Axes = new int[0],
            Buttons = new ButtonState[0],
            Hats = new JoystickHat[0]
        };
    }
}