// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteGamePad : GamePadStrategy
    {

        public override int PlatformGetMaxNumberOfGamePads()
        {
            throw new PlatformNotSupportedException();
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
