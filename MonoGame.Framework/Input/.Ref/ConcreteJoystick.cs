// Copyright (C)2022-2024 Nick Kastellanos

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
            get { throw new PlatformNotSupportedException(); }
        }

        public override JoystickCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        public override JoystickState PlatformGetState(int index)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformGetState(int index, ref JoystickState joystickState)
        {
            throw new PlatformNotSupportedException();
        }
    }
}

