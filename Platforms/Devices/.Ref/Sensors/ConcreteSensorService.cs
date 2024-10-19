// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteSensorService : SensorServiceStrategy
    {

        internal ConcreteSensorService()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformIsAccelerometerSupported()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformIsCompassSupported()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Suspend()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Resume()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}
