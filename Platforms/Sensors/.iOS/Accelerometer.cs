// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

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
        private TimeSpan _timeBetweenUpdates = TimeSpan.FromMilliseconds(2);
        private AccelerometerReading _currentValue;
        private SensorReadingEventArgs<AccelerometerReading> _eventArgs = new SensorReadingEventArgs<AccelerometerReading>(default(AccelerometerReading));

        static int _instanceCount;
        private static bool _started = false;
        private static SensorState _state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;


        internal static readonly CoreMotion.CMMotionManager _motionManager = new CoreMotion.CMMotionManager();


        public static bool IsSupported
        {
            get { return Accelerometer._motionManager.AccelerometerAvailable; }
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

        public override TimeSpan TimeBetweenUpdates
        {
            get { return _timeBetweenUpdates; }
            set
            {
                if (this._timeBetweenUpdates != value)
                {
                    this._timeBetweenUpdates = value;
                    Accelerometer._motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
                }
            }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return _currentValue; }
        }

        private static event CMAccelerometerHandler readingChanged;

        public Accelerometer()
        {
            if (!IsSupported)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
            else if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");

            ++_instanceCount;

            readingChanged += ReadingChangedHandler;
        }

        public override void Start()
        {
            if (_started == false)
            {
                Accelerometer._motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
                _started = true;
                _state = SensorState.Ready;
            }
            else
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
        }

        public override void Stop()
        {
            Accelerometer._motionManager.StopAccelerometerUpdates();
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
                _currentValue = reading;

                _eventArgs.SensorReading = _currentValue;
                OnCurrentValueChanged(_eventArgs);
            }
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

