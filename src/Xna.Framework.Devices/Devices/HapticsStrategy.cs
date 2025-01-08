// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public abstract class HapticsStrategy
    {
        public abstract void Vibrate(TimeSpan duration);
    }
}