// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    sealed partial class GamePad
    {
        private int PlatformGetMaxNumberOfGamePads()
        {
            throw new NotImplementedException();
        }

        private GamePadCapabilities PlatformGetCapabilities(int index)
        {
            throw new NotImplementedException();
        }

        private GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            throw new NotImplementedException();
        }

        private bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            throw new NotImplementedException();
        }
    }
}
