// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device’s accelerometer sensor.
    /// </summary>
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        const int MaxSensorCount = 10;

        private bool _isDisposed;
        private bool _isDataValid;

        static SensorManager _sensorManager;
        static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;
        bool _isRegistered;
        SensorState _state;
        bool _started = false;

        /// <summary>
        /// Gets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                if (_sensorManager == null)
                    Initialize();
                return _sensorAccelerometer != null;
            }
        }

        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().Name);

                if (_sensorManager == null)
                {
                    Initialize();
                    _state = _sensorAccelerometer != null ? SensorState.Initializing : SensorState.NotSupported;
                }
                return _state;
            }
        }

        protected override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public override bool IsDataValid
        {
            get { return _isDataValid; }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
            if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");
            ++_instanceCount;

            _state = _sensorAccelerometer != null ? SensorState.Initializing : SensorState.NotSupported;

            _sensorListener = new SensorListener();
            _sensorListener.AccuracyChanged += _sensorListener_AccuracyChanged;
            _sensorListener.SensorChanged += _sensorListener_SensorChanged;
        }

        /// <summary>
        /// Initializes the platform resources required for the accelerometer sensor.
        /// </summary>
        static void Initialize()
        {
            _sensorManager = (SensorManager)AndroidGameWindow.Activity.GetSystemService(Context.SensorService);
            _sensorAccelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        void _activity_Paused(object sender, EventArgs eventArgs)
        {
            _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
        }

        void _activity_Resumed(object sender, EventArgs eventArgs)
        {
            _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
        }

        private void _sensorListener_AccuracyChanged(object sender, EventArgs eventArgs)
        {
            //do nothing
        }

        private void _sensorListener_SensorChanged(object sender, SensorListener.SensorChangedEventArgs eventArgs)
        {
            try
            {
                SensorEvent e = eventArgs.Event;
                if (e != null && e.Sensor.Type == SensorType.Accelerometer && _isRegistered == true)
                {
                    IList<float> values = e.Values;
                    try
                    {
                        AccelerometerReading reading = new AccelerometerReading();
                        _isDataValid = (values != null && values.Count == 3);
                        if (_isDataValid)
                        {
                            const float gravity = SensorManager.GravityEarth;
                            reading.Acceleration = new Vector3(values[0], values[1], values[2]) / gravity;
                            reading.Timestamp = DateTime.UtcNow;
                        }
                        this.CurrentValue = reading;
                    }
                    finally
                    {
                        IDisposable d = values as IDisposable;
                        if (d != null)
                            d.Dispose();
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Occassionally an NullReferenceException is thrown when accessing e.Values??
                // mono    : Unhandled Exception: System.NullReferenceException: Object reference not set to an instance of an object
                // mono    :   at Android.Runtime.JNIEnv.GetObjectField (IntPtr jobject, IntPtr jfieldID) [0x00000] in <filename unknown>:0 
                // mono    :   at Android.Hardware.SensorEvent.get_Values () [0x00000] in <filename unknown>:0
            }
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        public override void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_sensorManager == null)
                Initialize();
            if (_started == false)
            {
                if (_sensorManager != null && _sensorAccelerometer != null)
                {
                    _isRegistered = true;
                    _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
                    // So the system can pause and resume the sensor when the activity is paused
                    AndroidGameWindow.Activity.Paused += _activity_Paused;
                    AndroidGameWindow.Activity.Resumed += _activity_Resumed;
                }
                else
                {
                    throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
                }
                _started = true;
                _state = SensorState.Ready;
                return;
            }
            else
            {
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
            }
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_started)
            {
                if (_sensorManager != null && _sensorAccelerometer != null)
                {
                    AndroidGameWindow.Activity.Paused -= _activity_Paused;
                    AndroidGameWindow.Activity.Resumed -= _activity_Resumed;
                    _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
                    _isRegistered = false;
                }
            }
            _started = false;
            _state = SensorState.Disabled;
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
                    if (_instanceCount == 0)
                    {
                        _sensorAccelerometer = null;
                        _sensorManager = null;
                    }
                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

