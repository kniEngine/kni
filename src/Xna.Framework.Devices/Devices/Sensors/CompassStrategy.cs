// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
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

        protected CompassReading CreateCompassReading(
            double headingAccuracy,
            double magneticHeading,
            Vector3 magnetometerReading,
            DateTimeOffset timestamp,
            double trueHeading
            )
        {
            CompassReading reading = new CompassReading();
            reading.HeadingAccuracy = headingAccuracy;
            reading.MagneticHeading = magneticHeading;
            reading.MagnetometerReading = magnetometerReading;
            reading.Timestamp = timestamp;
            reading.TrueHeading = trueHeading;
            return reading;
        }

    }
}