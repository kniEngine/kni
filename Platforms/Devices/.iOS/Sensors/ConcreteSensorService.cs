// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
        }

        public override bool PlatformIsAccelerometerSupported()
        {
            return ConcreteAccelerometer._motionManager.AccelerometerAvailable;
        }

        public override bool PlatformIsCompassSupported()
        {
            return ConcreteAccelerometer._motionManager.DeviceMotionAvailable;
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

