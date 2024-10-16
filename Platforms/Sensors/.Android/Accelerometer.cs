// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
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

        private bool _isDisposed;

        internal AccelerometerStrategy Strategy
        {
            get { return _strategy; }
        }

        /// <summary>
        /// Gets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        public static bool IsSupported
        {
            get { return SensorService.Current.IsAccelerometerSupported; }
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
            set { Strategy.TimeBetweenUpdates = value; }
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
        }

        private void _strategy_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> eventArgs)
        {
            OnCurrentValueChanged(eventArgs);
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
        /// </summary>
        public override void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            Strategy.Start();
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            Strategy.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Strategy.Dispose();

                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

