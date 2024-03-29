// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    sealed partial class GamePad
    {

        private int PlatformGetMaxNumberOfGamePads()
        {
            throw new PlatformNotSupportedException();
        }

        private GamePadCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            throw new PlatformNotSupportedException();
        }

        private bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
