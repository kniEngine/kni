// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteJoystick : JoystickStrategy
    {
        public override bool PlatformIsSupported
        {
            get { return false; }
        }

        public override int PlatformLastConnectedIndex
        {
            get { return -1; }
        }

        public override JoystickCapabilities PlatformGetCapabilities(int index)
        {
            return new JoystickCapabilities()
            {
                IsConnected = false,
                DisplayName = string.Empty,
                IsGamepad = false,
                AxisCount = 0,
                ButtonCount = 0,
                HatCount = 0
            };
        }

        public override JoystickState PlatformGetState(int index)
        {
            return JoystickStrategy.DefaultJoystickState;
        }


        public override void PlatformGetState(int index, ref JoystickState joystickState)
        {

        }
    }
}

