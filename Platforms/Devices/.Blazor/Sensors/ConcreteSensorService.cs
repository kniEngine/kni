// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;
using nkast.Wasm.Dom;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteSensorService : SensorServiceStrategy
    {

        internal ConcreteSensorService()
        {
        }

        public override bool PlatformIsAccelerometerSupported()
        {
            return false;
        }

        public override bool PlatformIsCompassSupported()
        {
            return Window.Current.IsDeviceOrientationEventDefined;
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
