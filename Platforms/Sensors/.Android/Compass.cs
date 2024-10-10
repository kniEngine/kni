// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device’s compass sensor.
    /// </summary>
    public sealed class Compass : SensorBase<CompassReading>
    {
        const int MaxSensorCount = 10;

        static SensorManager _sensorManager;
        static Sensor _sensorMagneticField;
        static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;
        bool _isRegistered;
        SensorState _state;
        bool _started = false;

        float[] _valuesAccelerometer;
        float[] _valuesMagenticField;
        float[] _matrixR;
        float[] _matrixI;
        float[] _matrixValues;

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
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name);
                if (_sensorManager == null)
                {
                    Initialize();
                }
                return _state;
            }
        }

        /// <summary>
        /// Creates a new instance of the Compass object.
        /// </summary>
        public Compass()
        {
            if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Compass class per application has been exceeded.");
            ++_instanceCount;

            _state = _sensorMagneticField != null ? SensorState.Initializing : SensorState.NotSupported;

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

                this.IsDataValid = SensorManager.GetRotationMatrix(_matrixR, _matrixI, _valuesAccelerometer, _valuesMagenticField);
                if (this.IsDataValid)
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
                    this.CurrentValue = reading;
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
            if (IsDisposed)
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
                _state = SensorState.Ready;
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
            if (IsDisposed)
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
            _state = SensorState.Disabled;
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
                    if (_instanceCount == 0)
                    {
                        _sensorAccelerometer = null;
                        _sensorMagneticField = null;
                        _sensorManager = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        class SensorListener : Java.Lang.Object, ISensorEventListener
        {
            public event EventHandler<EventArgs> AccuracyChanged;
            public event EventHandler<SensorListener.SensorChangedEventArgs> SensorChanged;

            public SensorListener()
            {
            }

            void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
                var handler = AccuracyChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            void ISensorEventListener.OnSensorChanged(SensorEvent e)
            {
                var handler = SensorChanged;
                if (handler != null)
                    handler(this, new SensorListener.SensorChangedEventArgs(e));
            }

            public class SensorChangedEventArgs : EventArgs
            {
                public readonly SensorEvent Event;

                internal SensorChangedEventArgs(SensorEvent sensorEvent)
                {
                    this.Event = sensorEvent;
                }
            }
        }
    }
}

