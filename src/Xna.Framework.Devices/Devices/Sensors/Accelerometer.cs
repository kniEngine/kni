// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Devices;
using Microsoft.Xna.Platform.Devices.Sensors;

namespace Microsoft.Xna.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides access to the device's accelerometer sensor.
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

        public override TimeSpan ReportInterval
        {
            get { return Strategy.ReportInterval; }
            set { Strategy.ReportInterval = value; }
        }

        public override AccelerometerReading CurrentReading
        {
            get { return Strategy.CurrentReading; }
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer object.
        /// </summary>
        public Accelerometer()
        {
            _strategy = DevicesFactory.Current.CreateAccelerometerStrategy();
            _strategy.ReadingChanged += _strategy_ReadingChanged;
        }

        private void _strategy_ReadingChanged(object sender, SensorReadingEventArgs<AccelerometerReading> eventArgs)
        {
            OnReadingChanged(eventArgs);
        }

        /// <summary>
        /// Starts data acquisition from the accelerometer.
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

            throw new ObjectDisposedException("Accelerometer");
        }
    }
}

