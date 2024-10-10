using System;

using Microsoft.Xna.Framework;

using CoreMotion;
using Foundation;

namespace Microsoft.Devices.Sensors
{
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        const int MaxSensorCount = 10;

        static int _instanceCount;
        private static bool _started = false;
        private static SensorState _state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;

        public static bool IsSupported
        {
            get { return _motionManager.AccelerometerAvailable; }
        }
        public SensorState State
        {
            get { return _state; }
        }

        private static event CMAccelerometerHandler readingChanged;

        public Accelerometer()
        {
            if (!IsSupported)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
            else if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");

            ++_instanceCount;

            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_started)
                        Stop();
                    --_instanceCount;
                }
            }
            base.Dispose(disposing);
        }

        public override void Start()
        {
            if (_started == false)
            {
                _motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
                _started = true;
                _state = SensorState.Ready;
            }
            else
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
        }

        public override void Stop()
        {
            _motionManager.StopAccelerometerUpdates();
            _started = false;
            _state = SensorState.Disabled;
        }

        private void AccelerometerHandler(CMAccelerometerData data, NSError error)
        {
            readingChanged(data, error);
        }

        private void ReadingChangedHandler(CMAccelerometerData data, NSError error)
        {
            AccelerometerReading reading = new AccelerometerReading();
            this.IsDataValid = error == null;
            if (this.IsDataValid)
            {
                this.IsDataValid = true;
                reading.Acceleration = new Vector3((float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z);
                reading.Timestamp = DateTime.UtcNow;
                this.CurrentValue = reading;
                this.IsDataValid = error == null;
            }
        }

        private void UpdateInterval(object sender, EventArgs args)
        {
            _motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
        }
    }
}

