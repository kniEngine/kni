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