// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;
using Android.Hardware;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteSensorService : SensorServiceStrategy
    {

        internal ConcreteSensorService()
        {
        }

        public override bool PlatformIsAccelerometerSupported()
        {
            if (ConcreteAccelerometer._sensorManager == null)
                ConcreteAccelerometer.Initialize();
            return ConcreteAccelerometer._sensorAccelerometer != null;
        }

        public override bool PlatformIsCompassSupported()
        {
            if (ConcreteCompass._sensorManager == null)
                ConcreteCompass.Initialize();
            return ConcreteCompass._sensorMagneticField != null;
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

