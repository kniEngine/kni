// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;
using WinSensors = Windows.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteAccelerometer : AccelerometerStrategy
    {
        internal WinSensors.Accelerometer _waccelerometer;

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
            set
            {
                base.TimeBetweenUpdates = value;
                _waccelerometer.ReportInterval = (uint)value.TotalMilliseconds;
            }
        }

        public override AccelerometerReading CurrentValue
        {
            get
            {
                WinSensors.AccelerometerReading wreading = _waccelerometer.GetCurrentReading();
                if (wreading == null)
                {
                    base.IsDataValid = false;
                    return new AccelerometerReading();
                }

                AccelerometerReading reading = base.CreateAccelerometerReading(
                    acceleration: new Vector3((float)wreading.AccelerationX, (float)wreading.AccelerationY, (float)wreading.AccelerationZ),
                    // timestamp: war.Timestamp //cause Memory garbage
                    timestamp: DateTimeOffset.MinValue
                );
                base.IsDataValid = true;
                return reading;
            }
            set { base.CurrentValue = value; }
        }

        public ConcreteAccelerometer()
        {
            _waccelerometer = WinSensors.Accelerometer.GetDefault();
            if (_waccelerometer == null)
            {
                base.State = SensorState.NotSupported;
                return;
            }

            base.State = SensorState.Disabled;
        }

        public override void Start()
        {
            if (this.State != SensorState.Ready)
            {
                _waccelerometer.ReadingChanged += _waccelerometer_ReadingChanged;

                base.State = SensorState.Ready;
            }
        }

        public override void Stop()
        {
            if (this.State == SensorState.Ready)
            {
                _waccelerometer.ReadingChanged -= _waccelerometer_ReadingChanged;


                base.State = SensorState.Disabled;
            }
        }

        private void _waccelerometer_ReadingChanged(WinSensors.Accelerometer sender, WinSensors.AccelerometerReadingChangedEventArgs args)
        {
            WinSensors.AccelerometerReading wreading = args.Reading;

            if (wreading == null)
            {
                base.IsDataValid = false;
                return;
            }

            AccelerometerReading reading = base.CreateAccelerometerReading(
                   acceleration: new Vector3((float)wreading.AccelerationX, (float)wreading.AccelerationY, (float)wreading.AccelerationZ),
                   // timestamp: war.Timestamp //cause Memory garbage
                   timestamp: DateTimeOffset.MinValue
               );
            base.IsDataValid = true;

            base.CurrentValue = reading;

            var eventArgs = new SensorReadingEventArgs<AccelerometerReading>(reading);
            base.OnCurrentValueChanged(eventArgs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (this.State == SensorState.Ready)
                Stop();

            base.Dispose(disposing);
        }
    }
}
