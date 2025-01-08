// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteHaptics : HapticsStrategy
    {

        public ConcreteHaptics()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Vibrate(TimeSpan duration)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
