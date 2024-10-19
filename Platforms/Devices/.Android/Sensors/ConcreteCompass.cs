// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    internal sealed class ConcreteCompass : CompassStrategy
    {
        internal static SensorManager _sensorManager;
        internal static Sensor _sensorMagneticField;
        static Sensor _sensorAccelerometer;
        static int _instanceCount;

        SensorListener _sensorListener;

        float[] _valuesAccelerometer;
        float[] _valuesMagenticField;
        float[] _matrixR;
        float[] _matrixI;
        float[] _matrixValues;
        
        private SensorReadingEventArgs<CompassReading> _eventArgs = new SensorReadingEventArgs<CompassReading>(default(CompassReading));


        public override SensorState State
        {
            get
            {
                if (_sensorManager == null)
                {
                    ConcreteCompass.Initialize();
                }

                return base.State;
            }
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
                if (base.TimeBetweenUpdates != value)
                {
                    base.TimeBetweenUpdates = value;
                    // TODO: implement TimeBetweenUpdates for Android
                }
            }
        }

        public override CompassReading CurrentReading
        {
            get { return base.CurrentReading; }
            set { base.CurrentReading = value; }
        }


        public ConcreteCompass()
        {
            _instanceCount++;

            base.State = (_sensorMagneticField != null)
                       ? SensorState.Initializing 
                       : SensorState.NotSupported;

            _valuesAccelerometer = new float[3];
            _valuesMagenticField = new float[3];
            _matrixR = new float[9];
            _matrixI = new float[9];
            _matrixValues = new float[3];
            _sensorListener = new SensorListener();
            _sensorListener.AccuracyChanged += _sensorListener_AccuracyChanged;
            _sensorListener.SensorChanged += _sensorListener_SensorChanged;
        }

        static internal void Initialize()
        {
            _sensorManager = (SensorManager)AndroidGameWindow.Activity.GetSystemService(Context.SensorService);
            _sensorMagneticField = _sensorManager.GetDefaultSensor(SensorType.MagneticField);
            _sensorAccelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        public override void Start()
        {
            if (this.State == SensorState.Ready)
                throw base.CreateSensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");

            if (_sensorManager == null)
                ConcreteCompass.Initialize();

            if ((_sensorManager == null || _sensorMagneticField == null || _sensorAccelerometer == null))
                throw base.CreateSensorFailedException("Failed to start compass data acquisition. No default sensor found.");

            _sensorManager.RegisterListener(_sensorListener, _sensorMagneticField, SensorDelay.Game);
            _sensorManager.RegisterListener(_sensorListener, _sensorAccelerometer, SensorDelay.Game);
            base.State = SensorState.Ready;
        }

        public override void Stop()
        {
            if (this.State == SensorState.Ready)
            {
                if (_sensorManager != null && _sensorMagneticField != null && _sensorAccelerometer != null)
                {
                    _sensorManager.UnregisterListener(_sensorListener, _sensorAccelerometer);
                    _sensorManager.UnregisterListener(_sensorListener, _sensorMagneticField);
                }
            }
            base.State = SensorState.Disabled;
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

                base.IsDataValid = SensorManager.GetRotationMatrix(_matrixR, _matrixI, _valuesAccelerometer, _valuesMagenticField);
                if (base.IsDataValid)
                {
                    SensorManager.GetOrientation(_matrixR, _matrixValues);

                    double headingAccuracy = 0; // Not implemented.
                    double magneticHeading = _matrixValues[0];
                    Vector3 magnetometerReading = new Vector3(_valuesMagenticField[0], _valuesMagenticField[1], _valuesMagenticField[2]);
                    // We need the magnetic declination from true north to calculate the true heading from the magnetic heading.
                    // On Android, this is available through Android.Hardware.GeomagneticField, but this requires your geo position.
                    double trueHeading = magneticHeading; // Not implemented, fallback to magneticHeading.

                    CompassReading reading = base.CreateCompassReading(
                        headingAccuracy: headingAccuracy,
                        magneticHeading: magneticHeading,
                        magnetometerReading: magnetometerReading,
                        timestamp: DateTime.UtcNow,
                        trueHeading: trueHeading
                        );

                    base.CurrentReading = reading;

                    _eventArgs.SensorReading = base.CurrentReading;
                    base.OnReadingChanged(_eventArgs);
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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (this.State == SensorState.Ready)
                Stop();

            _instanceCount--;
            if (_instanceCount == 0)
            {
                _sensorAccelerometer = null;
                _sensorMagneticField = null;
                _sensorManager = null;
            }

            base.Dispose(disposing);
        }

    }
}