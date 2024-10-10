// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Input.Sensors;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device’s compass sensor.
    /// </summary>
    public sealed class Compass : SensorBase<CompassReading>
    {
        private CompassStrategy _strategy;

        const int MaxSensorCount = 10;

        private bool _isDisposed;
        private SensorReadingEventArgs<CompassReading> _eventArgs = new SensorReadingEventArgs<CompassReading>(default(CompassReading));

        static SensorManager _sensorManager;
        static Sensor _sensorMagneticField;
        static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;
        bool _isRegistered;
        bool _started = false;

        float[] _valuesAccelerometer;
        float[] _valuesMagenticField;
        float[] _matrixR;
        float[] _matrixI;
        float[] _matrixValues;

        internal CompassStrategy Strategy
        {
            get { return _strategy; }
        }

        /// <summary>
        /// Gets whether the device on which the application is running supports the compass sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                if (_sensorManager == null)
                    Initialize();
                return _sensorMagneticField != null;
            }
        }

        /// <summary>
        /// Gets the current state of the compass. The value is a member of the SensorState enumeration.
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

        public override CompassReading CurrentValue
        {
            get { return Strategy.CurrentValue; }
        }

        /// <summary>
        /// Creates a new instance of the Compass object.
        /// </summary>
        public Compass()
        {
            _strategy = new CompassStrategy();
            _strategy.CurrentValueChanged += _strategy_CurrentValueChanged;
            _strategy.Calibrate += _strategy_Calibrate;

            if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Compass class per application has been exceeded.");
            ++_instanceCount;

            Strategy.State = _sensorMagneticField != null ? SensorState.Initializing : SensorState.NotSupported;

            _valuesAccelerometer = new float[3];
            _valuesMagenticField = new float[3];
            _matrixR = new float[9];
            _matrixI = new float[9];
            _matrixValues = new float[3];
            _sensorListener = new SensorListener();
            _sensorListener.AccuracyChanged += _sensorListener_AccuracyChanged;
            _sensorListener.SensorChanged += _sensorListener_SensorChanged;
        }

        /// <summary>
        /// Initializes the platform resources required for the compass sensor.
        /// </summary>
        static void Initialize()
        {
            _sensorManager = (SensorManager)AndroidGameWindow.Activity.GetSystemService(Context.SensorService);
            _sensorMagneticField = _sensorManager.GetDefaultSensor(SensorType.MagneticField);
            _sensorAccelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        private void _strategy_CurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> eventArgs)
        {
            OnCurrentValueChanged(eventArgs);
        }

        private void _strategy_Calibrate(object sender, CalibrationEventArgs eventArgs)
        {
            OnCalibrate(eventArgs);
        }


        void _activity_Paused(object sender, EventArgs eventArgs)
        {
            _sensorManager.UnregisterListener(_sensorListener, _sensorMagneticField);
            _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
        }

        void _activity_Resumed(object sender, EventArgs eventArgs)
        {
            _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
            _sensorManager.RegisterListener(_sensorListener, _sensorMagneticField, SensorDelay.Game);
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
                switch (e.Sensor.Type)
                {
                    case SensorType.Accelerometer:
                        _valuesAccelerometer[0] = e.Values[0];
                        _valuesAccelerometer[1] = e.Values[1];
                        _valuesAccelerometer[2] = e.Values[2];
                        break;

                    case SensorType.MagneticField:
                        _valuesMagenticField[0] = e.Values[0];
                        _valuesMagenticField[1] = e.Values[1];
                        _valuesMagenticField[2] = e.Values[2];
                        break;
                }

                Strategy.IsDataValid = SensorManager.GetRotationMatrix(_matrixR, _matrixI, _valuesAccelerometer, _valuesMagenticField);
                if (Strategy.IsDataValid)
                {
                    SensorManager.GetOrientation(_matrixR, _matrixValues);
                    CompassReading reading = new CompassReading();
                    reading.MagneticHeading = _matrixValues[0];
                    Vector3 magnetometer = new Vector3(_valuesMagenticField[0], _valuesMagenticField[1], _valuesMagenticField[2]);
                    reading.MagnetometerReading = magnetometer;
                    // We need the magnetic declination from true north to calculate the true heading from the magnetic heading.
                    // On Android, this is available through Android.Hardware.GeomagneticField, but this requires your geo position.
                    reading.TrueHeading = reading.MagneticHeading;
                    reading.Timestamp = DateTime.UtcNow;
                    Strategy.CurrentValue = reading;

                    _eventArgs.SensorReading = Strategy.CurrentValue;
                    Strategy.OnCurrentValueChanged(_eventArgs);
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
        /// Starts data acquisition from the compass.
        /// </summary>
        public override void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_sensorManager == null)
                Initialize();
            if (_started == false)
            {
                if (_sensorManager != null && _sensorMagneticField != null && _sensorAccelerometer != null)
                {
                    _isRegistered = true;
                    _sensorManager.RegisterListener(_sensorListener, _sensorMagneticField, SensorDelay.Game);
                    _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
                }
                else
                {
                    throw new SensorFailedException("Failed to start compass data acquisition. No default sensor found.");
                }
                _started = true;
                Strategy.State = SensorState.Ready;
                return;
            }
            else
            {
                throw new SensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");
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
                if (_sensorManager != null && _sensorMagneticField != null && _sensorAccelerometer != null)
                {
                    _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
                    _sensorManager.UnregisterListener(_sensorListener, _sensorMagneticField);
                    _isRegistered = false;
                }
            }
            _started = false;
            Strategy.State = SensorState.Disabled;
        }

        private void OnCalibrate(CalibrationEventArgs eventArgs)
        {
            //var handler = Calibrate;
            //if (handler != null)
            //    handler(this, eventArgs);
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
                        _sensorMagneticField = null;
                        _sensorManager = null;
                    }
                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

