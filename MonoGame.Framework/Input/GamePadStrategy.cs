// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class GamePadStrategy
    {
        public abstract int PlatformGetMaxNumberOfGamePads();
        public abstract GamePadCapabilities PlatformGetCapabilities(int index);
        public abstract GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode);
        public abstract bool PlatformSetVibration(int index, float v1, float v2, float v3, float v4);
    }
}