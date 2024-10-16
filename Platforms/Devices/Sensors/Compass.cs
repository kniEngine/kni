// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Platform.Devices.Sensors;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides access to the device's compass sensor.
    /// </summary>
    public sealed class Compass : SensorBase<CompassReading>
    {
        private CompassStrategy _strategy;

        private bool _isDisposed;

        public event EventHandler<CalibrationEventArgs> Calibrate;

        internal CompassStrategy Strategy
        {
            get { return _strategy; }
        }

        /// <summary>
        /// Gets whether the device on which the application is running supports the compass sensor.
        /// </summary>
        public static bool IsSupported
        {
            get { return SensorService.Current.IsCompassSupported; }
        }

        /// <summary>
        /// Gets the current state of the compass. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                ThrowIfDisposed();

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

        public override CompassReading CurrentValue
        {
            get { return Strategy.CurrentValue; }
        }

        /// <summary>
        /// Creates a new instance of the Compass object.
        /// </summary>
        public Compass()
        {
            _strategy = new ConcreteCompass();
            _strategy.CurrentValueChanged += _strategy_CurrentValueChanged;
            _strategy.Calibrate += _strategy_Calibrate;
        }

        private void _strategy_CurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> eventArgs)
        {
            OnCurrentValueChanged(eventArgs);
        }

        private void _strategy_Calibrate(object sender, CalibrationEventArgs eventArgs)
        {
            OnCalibrate(eventArgs);
        }

        /// <summary>
        /// Starts data acquisition from the compass.
        /// </summary>
        public override void Start()
        {
            ThrowIfDisposed();

            Strategy.Start();
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            ThrowIfDisposed();

            Strategy.Stop();
        }

        private void OnCalibrate(CalibrationEventArgs eventArgs)
        {
            var handler = Calibrate;
            if (handler != null)
                handler(this, eventArgs);
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

        private void ThrowIfDisposed()
        {
            if (!_isDisposed)
                return;

            throw new ObjectDisposedException("Compass");
        }
    }
}

