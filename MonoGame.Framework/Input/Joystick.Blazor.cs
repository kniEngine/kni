// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    sealed partial class Joystick
    {
        private bool PlatformIsSupported
        {
            get { return false; }
        }

        private static int PlatformLastConnectedIndex
        {
            get { return -1; }
        }

        private JoystickCapabilities PlatformGetCapabilities(int index)
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

        private JoystickState PlatformGetState(int index)
        {
            return Joystick.DefaultJoystickState;
        }


        private void PlatformGetState(int index, ref JoystickState joystickState)
        {

        }
    }
}

