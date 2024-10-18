// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteDevicesFactory : DevicesFactory
    {

        public override VibratorStrategy CreateVibratorStrategy()
        {
            return new ConcreteVibrator();
        }

    }
}