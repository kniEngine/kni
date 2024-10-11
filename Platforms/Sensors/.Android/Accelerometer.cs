// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Input.Sensors;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device’s accelerometer sensor.
    /// </summary>
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        private AccelerometerStrategy _strategy;

        const int MaxSensorCount = 10;

        private bool _isDisposed;
        private SensorReadingEventArgs<AccelerometerReading> _eventArgs = new SensorReadingEventArgs<AccelerometerReading>(default(AccelerometerReading));

        static SensorManager _sensorManager;
        static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;
        bool _isRegistered;
        bool _started = false;

        internal AccelerometerStrategy Strategy
        {
            get { return _strategy; }
        }

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
                    Strategy.State = _sensorAccelerometer != null ? SensorState.Initializing : SensorState.NotSupported;
                }                
                return Strategy.State;
            }
        }

        protected override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public override bool IsDataValid
        {
            get { return Strategy.IsDataValid; }
        }

        public override TimeSpan TimeBetweenUpdates
        {
            get { return Strategy.TimeBetweenUpdates; }
            set
            {
                if (Strategy.TimeBetweenUpdates != value)
                {
                    Strategy.TimeBetweenUpdates = value;
                    // TODO: implement TimeBetweenUpdates for Android
                }
            }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return Strategy.CurrentValue; }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
            _strategy = new ConcreteAccelerometer();
            _strategy.CurrentValueChanged += _strategy_CurrentValueChanged;

            if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");
            ++_instanceCount;

            Strategy.State = _sensorAccelerometer != null ? SensorState.Initializing : SensorState.NotSupported;

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

        private void _strategy_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> eventArgs)
        {
            OnCurrentValueChanged(eventArgs);
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
                        Strategy.IsDataValid = (values != null && values.Count == 3);
                        if (Strategy.IsDataValid)
                        {
                            const float gravity = SensorManager.GravityEarth;
                            reading.Acceleration = new Vector3(values[0], values[1], values[2]) / gravity;
                            reading.Timestamp = DateTime.UtcNow;
                        }
                        Strategy.CurrentValue = reading;

                        _eventArgs.SensorReading = Strategy.CurrentValue;
                        Strategy.OnCurrentValueChanged(_eventArgs);
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

            Strategy.Start();

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
                Strategy.State = SensorState.Ready;
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

            Strategy.Stop();

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
            Strategy.State = SensorState.Disabled;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Strategy.Dispose();

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

