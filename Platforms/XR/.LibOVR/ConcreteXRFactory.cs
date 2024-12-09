// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.XR;

namespace Microsoft.Xna.Platform.XR.LibOVR
{
    public sealed class ConcreteXRFactory : XRFactory
    {
        public override XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, IServiceProvider services, XRMode mode)
        {
            return new ConcreteXRDevice(applicationName, services, mode);
        }

        public override XRDeviceStrategy CreateXRDeviceStrategy(string applicationName, Game game, XRMode mode)
        {
            return new ConcreteXRDevice(applicationName, game, mode);
        }
    }
}
