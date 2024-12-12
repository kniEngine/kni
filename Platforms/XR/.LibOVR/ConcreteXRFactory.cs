// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.XR;

namespace Microsoft.Xna.Platform.XR.LibOVR
{
    public sealed class ConcreteXRFactory : XRFactory
    {
        public override XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, IServiceProvider services)
        {
            return new ConcreteXRDevice(applicationName, services);
        }

        public override XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, Game game)
        {
            return new ConcreteXRDevice(applicationName, game);
        }
    }
}
