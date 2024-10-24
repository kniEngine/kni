// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    public abstract class AccelerometerStrategy : SensorStrategy<AccelerometerReading>
        , IDisposable
    {

        public AccelerometerStrategy()
        {
        }

        protected AccelerometerReading CreateAccelerometerReading(
            Vector3 acceleration,
            DateTimeOffset timestamp
            )
        {
            AccelerometerReading reading = new AccelerometerReading();
            reading.Acceleration = acceleration;
            reading.Timestamp = timestamp;
            return reading;
        }

        protected AccelerometerFailedException CreateAccelerometerFailedException(string message, int errorId)
        {            
            return new AccelerometerFailedException(message, errorId);
        }
    }
}
