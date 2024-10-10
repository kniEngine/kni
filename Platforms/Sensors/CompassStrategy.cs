// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Devices.Sensors
{
    public class CompassStrategy : SensorStrategy<CompassReading>
        , IDisposable
    {

        public event EventHandler<CalibrationEventArgs> Calibrate;

        public CompassStrategy()
        {
        }

        internal
        protected virtual void OnCalibrate(CalibrationEventArgs eventArgs)
        {
            var handler = Calibrate;
            if (handler != null)
                handler(this, eventArgs);
        }

    }
}