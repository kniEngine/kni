// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Devices.Sensors
{
    public class AccelerometerStrategy : SensorStrategy<AccelerometerReading>
        , IDisposable
    {

        public AccelerometerStrategy()
        {
        }

    }
}
