// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Devices.Sensors
{
    public class CompassStrategy : SensorStrategy<CompassReading>
        , IDisposable
    {

        public CompassStrategy()
        {
        }

    }
}