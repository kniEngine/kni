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

        static SensorManager _sensorManager;
        static Sensor _sensor;
        static int _instanceCount;

        SensorListener _sensorListener;
        SensorState _state;
        bool _started = false;

        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                if (_sensorManager == null)
                    Initialize();
                return _sensor != null;
            }
        }

        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
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
                    _state = _sensor != null ? SensorState.Initializing : SensorState.NotSupported;
                }
                return _state;
            }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
            if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");
            ++_instanceCount;

            _state = _sensor != null ? SensorState.Initializing : SensorState.NotSupported;

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
            _sensor = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        void _activity_Paused(object sender, EventArgs eventArgs)
        {
            _sensorManager.UnregisterListener(_sensorListener, _sensor);
        }

        void _activity_Resumed(object sender, EventArgs eventArgs)
        {
            _sensorManager.RegisterListener(_sensorListener, _sensor, SensorDelay.Game);

        }

        private void _sensorListener_AccuracyChanged(object sender, EventArgs eventArgs)
        {
            //do nothing
        }

        private void _sensorListener_SensorChanged(object sender, SensorListener.SensorChangedEventArgs eventArgs)
        {
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_sensorManager == null)
                Initialize();
            if (_started == false)
            {
                if (_sensorManager != null && _sensor != null)
                {
                    _sensorListener._accelerometer = this;
                    _sensorManager.RegisterListener(_sensorListener, _sensor, SensorDelay.Game);
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
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_started)
            {
                if (_sensorManager != null && _sensor != null)
                {
                    AndroidGameWindow.Activity.Paused -= _activity_Paused;
                    AndroidGameWindow.Activity.Resumed -= _activity_Resumed;
                    _sensorManager.UnregisterListener(_sensorListener, _sensor);
                    _sensorListener._accelerometer = null;
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
                        _sensor = null;
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

            internal Accelerometer _accelerometer;

            public SensorListener()
            {
            }

            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
                var handler = AccuracyChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            public void OnSensorChanged(SensorEvent e)
            {
                var handler = SensorChanged;
                if (handler != null)
                    handler(this, new SensorListener.SensorChangedEventArgs(e));

                try
                {
                    if (e != null && e.Sensor.Type == SensorType.Accelerometer && _accelerometer != null)
                    {
                        IList<float> values = e.Values;
                        try
                        {
                            AccelerometerReading reading = new AccelerometerReading();
                            _accelerometer.IsDataValid = (values != null && values.Count == 3);
                            if (_accelerometer.IsDataValid)
                            {
                                const float gravity = SensorManager.GravityEarth;
                                reading.Acceleration = new Vector3(values[0], values[1], values[2]) / gravity;
                                reading.Timestamp = DateTime.UtcNow;
                            }
                            _accelerometer.CurrentValue = reading;
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

