// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private const bool PlatformIsSupported = false;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private static JoystickState PlatformGetState(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private static int PlatformLastConnectedIndex
        {
            get { throw new PlatformNotSupportedException(); }
        }

        private static void PlatformGetState(ref JoystickState joystickState, int index)
        {
            throw new PlatformNotSupportedException();
        }
    }
}

