// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {

        static GamePad()
        {
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            throw new PlatformNotSupportedException();
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            throw new PlatformNotSupportedException();
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            throw new PlatformNotSupportedException();
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
