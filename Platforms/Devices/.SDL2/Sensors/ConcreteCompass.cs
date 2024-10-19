// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteCompass : CompassStrategy
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

        public override TimeSpan ReportInterval
        {
            get { return base.ReportInterval; }
            set { base.ReportInterval = value; }
        }

        public override CompassReading CurrentReading
        {
            get { return base.CurrentReading; }
            set { base.CurrentReading = value; }
        }


        public ConcreteCompass()
        {
            base.State = SensorState.NotSupported;

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
