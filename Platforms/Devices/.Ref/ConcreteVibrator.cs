// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteVibrator : VibratorStrategy
    {

        public ConcreteVibrator()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Vibrate(TimeSpan duration)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
