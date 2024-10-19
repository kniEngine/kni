// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    public class AccelerometerStrategy : SensorStrategy<AccelerometerReading>
        , IDisposable
    {

        public AccelerometerStrategy()
        {
        }

        protected AccelerometerFailedException CreateAccelerometerFailedException(string message, int errorId)
        {            
            return new AccelerometerFailedException(message, errorId);
        }
    }
}
