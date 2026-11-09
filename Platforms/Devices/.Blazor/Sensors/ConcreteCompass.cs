// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;
using nkast.Wasm.Dom;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteCompass : CompassStrategy
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

        public override CompassReading CurrentReading
        {
            get { return base.CurrentReading; }
            set { base.CurrentReading = value; }
        }

        public ConcreteCompass()
        {
            base.State = Window.Current.IsDeviceOrientationEventDefined
                       ? SensorState.Disabled
                       : SensorState.NotSupported;
        }

        public override void Start()
        {
            if (this.State == SensorState.Ready)
                throw base.CreateSensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");

            if (this.State == SensorState.Disabled
            ||  this.State == SensorState.NoPermissions)
            {
                this.State = SensorState.Initializing;
                Task.Run(async () =>
                {
                    string permission = await Window.Current.DeviceOrientationEventRequestPermissionAsync(true);
                    if (permission == "granted")
                    {
                        Window.Current.OnDeviceOrientation += _domWindow_OnDeviceOrientation;
                        if (_instanceStartedCount == 0)
                            Window.Current.RegisterDeviceOrientationEvent();
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
                Window.Current.OnDeviceOrientation -= _domWindow_OnDeviceOrientation;
                _instanceStartedCount--;

                if (_instanceStartedCount == 0)
                    Window.Current.UnregisterDeviceOrientationEvent();
            }

            base.State = SensorState.Disabled;
        }

        private void _domWindow_OnDeviceOrientation(object sender, float? alpha, float? beta, float? gamma, bool absolute, float? webkitCompassHeading)
        {
            CompassReading reading = new CompassReading();

            bool orientationDataValid = absolute && alpha.HasValue && beta.HasValue && gamma.HasValue;
            base.IsDataValid = orientationDataValid || webkitCompassHeading.HasValue;

            if (base.IsDataValid)
            {
                float magneticHeading;
                if (orientationDataValid)
                {
                    Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(
                        MathHelper.ToRadians(-alpha.Value),
                        MathHelper.ToRadians(-beta.Value),
                        MathHelper.ToRadians(-gamma.Value));
                    if (Math.Abs(rotationMatrix.Backward.Y) < Math.Abs(rotationMatrix.Down.Y))
                        magneticHeading = MathF.Atan2(rotationMatrix.Backward.X, rotationMatrix.Backward.Z);
                    else
                        magneticHeading = MathF.Atan2(rotationMatrix.Down.X, rotationMatrix.Down.Z);
                }
                else
                {
                    magneticHeading = webkitCompassHeading.Value;
                }

                reading = base.CreateCompassReading(
                    headingAccuracy: 0,                // Not available on web
                    magneticHeading: magneticHeading,
                    magnetometerReading: Vector3.Zero, // Not available on web
                    timestamp: DateTime.UtcNow,
                    trueHeading: magneticHeading       // Not available on web, fallback to magneticHeading
                );
            }

            base.CurrentReading = reading;
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
