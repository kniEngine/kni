// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Devices.Sensors;

namespace Microsoft.Xna.Platform.Input.Sensors
{
    public class CompassStrategy : SensorStrategy<CompassReading>
        , IDisposable
    {

        public CompassStrategy()
        {
        }

    }
}