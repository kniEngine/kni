// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;
using nkast.Wasm.Dom;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteAccelerometer : AccelerometerStrategy
    {
        static int _instanceStartedCount;

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

        public override AccelerometerReading CurrentReading
        {
            get { return base.CurrentReading; }
            set { base.CurrentReading = value; }
        }

        public ConcreteAccelerometer()
        {
            base.State = Window.Current.IsDeviceMotionEventDefined
                       ? SensorState.Disabled
                       : SensorState.NotSupported;
        }

        public override void Start()
        {
            if (this.State == SensorState.Ready)
                throw base.CreateAccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);

            if (this.State == SensorState.Disabled
            ||  this.State == SensorState.NoPermissions)
            {
                this.State = SensorState.Initializing;
                Task.Run(async () =>
                {
                    string permission = await Window.Current.DeviceMotionEventRequestPermissionAsync();
                    if (permission == "granted")
                    {
                        Window.Current.OnDeviceMotion += _domWindow_OnDeviceMotion;
                        if (_instanceStartedCount == 0)
                            Window.Current.RegisterDeviceMotionEvent();
                        this.State = SensorState.Ready;
                        _instanceStartedCount++;
                    }
                    else
                    {
                        this.State = SensorState.NoPermissions;
                    }
                });
            }
        }

        public override void Stop()
        {
            if (this.State == SensorState.Ready)
            {
                Window.Current.OnDeviceMotion -= _domWindow_OnDeviceMotion;
                _instanceStartedCount--;

                if (_instanceStartedCount == 0)
                    Window.Current.UnregisterDeviceMotionEvent();
            }

            base.State = SensorState.Disabled;
        }

        private void _domWindow_OnDeviceMotion(object sender, float? x, float? y, float? z, float interval)
        {
            AccelerometerReading reading = new AccelerometerReading();
            base.IsDataValid = x.HasValue && y.HasValue && z.HasValue;
            if (base.IsDataValid)
            {
                const float gravity = 9.80665f;
                reading = base.CreateAccelerometerReading(
                    acceleration: new Vector3(x.Value, y.Value, z.Value) / gravity,
                    timestamp: DateTime.UtcNow
                );
            }

            base.CurrentReading = reading;
            base.ReportInterval = TimeSpan.FromMilliseconds(interval);
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
