using System;

using Microsoft.Xna.Framework;

using CoreMotion;
using Foundation;

namespace Microsoft.Devices.Sensors
{
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        const int MaxSensorCount = 10;

        private bool _isDisposed;
        private bool _isDataValid;

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

        protected override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public override bool IsDataValid
        {
            get { return _isDataValid; }
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
            _isDataValid = (error == null);
            if (_isDataValid)
            {
                reading.Acceleration = new Vector3((float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z);
                reading.Timestamp = DateTime.UtcNow;
                this.CurrentValue = reading;
            }
        }

        private void UpdateInterval(object sender, EventArgs args)
        {
            _motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_started)
                        Stop();
                    --_instanceCount;
                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

