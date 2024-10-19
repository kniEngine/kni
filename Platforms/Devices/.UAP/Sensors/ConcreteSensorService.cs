// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;
using WinSensors = Windows.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteSensorService : SensorServiceStrategy
    {
        bool _isAccelerometerSupported;

        internal ConcreteSensorService()
        {
            WinSensors.Accelerometer waccelerometer = WinSensors.Accelerometer.GetDefault();
            _isAccelerometerSupported = (waccelerometer != null);
        }

        public override bool PlatformIsAccelerometerSupported()
        {
            return _isAccelerometerSupported;
        }

        public override bool PlatformIsCompassSupported()
        {
            return false;
        }

        public override void Suspend()
        {
        }

        public override void Resume()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}
