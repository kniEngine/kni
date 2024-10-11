// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Devices.Sensors;

namespace Microsoft.Xna.Platform.Input.Sensors
{
    internal class ConcreteAccelerometer : AccelerometerStrategy
    {

        public override SensorState State
        {
            get { return base.State; }
            set { base.State = value; }
        }

        public override bool IsDataValid
        {
            get { return base.IsDataValid; }
            set { base.IsDataValid = value; }
        }

        public override TimeSpan TimeBetweenUpdates
        {
            get { return base.TimeBetweenUpdates; }
            set { base.TimeBetweenUpdates = value; }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return base.CurrentValue; }
            set { base.CurrentValue = value; }
        }

        public ConcreteAccelerometer()
        {
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}