// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    sealed partial class Joystick
    {
        private bool PlatformIsSupported
        {
            get { return false; } 
        }

        private int PlatformLastConnectedIndex
        {
            get { throw new PlatformNotSupportedException(); }
        }

        private JoystickCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private JoystickState PlatformGetState(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetState(int index, ref JoystickState joystickState)
        {
            throw new PlatformNotSupportedException();
        }
    }
}

