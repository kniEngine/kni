﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Devices.Sensors;
using CoreMotion;
using Foundation;

namespace Microsoft.Platform.Devices.Sensors
{
    internal class ConcreteAccelerometer : AccelerometerStrategy
    {
        static int _instanceCount;
        private static SensorState _state = (SensorService.Current.IsAccelerometerSupported)
                                          ? SensorState.Initializing
                                          : SensorState.NotSupported;

        internal static readonly CoreMotion.CMMotionManager _motionManager = new CoreMotion.CMMotionManager();

        private static event CMAccelerometerHandler readingChanged;

        private SensorReadingEventArgs<AccelerometerReading> _eventArgs = new SensorReadingEventArgs<AccelerometerReading>(default(AccelerometerReading));


        public override SensorState State
        {
            get
            {
                return _state;
                //return base.State;
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
                    ConcreteAccelerometer._motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
                }
            }
        }

        public override AccelerometerReading CurrentValue
        {
            get { return base.CurrentValue; }
            set { base.CurrentValue = value; }
        }


        public ConcreteAccelerometer()
        {
            if (!SensorService.Current.IsAccelerometerSupported)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);

            _instanceCount++;

            readingChanged += ReadingChangedHandler;
        }

        public override void Start()
        {
            if (this.State == SensorState.Ready)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);

            ConcreteAccelerometer._motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
            _state = SensorState.Ready;
        }

        public override void Stop()
        {
            ConcreteAccelerometer._motionManager.StopAccelerometerUpdates();
            _state = SensorState.Disabled;
        }

        private void AccelerometerHandler(CMAccelerometerData data, NSError error)
        {
            readingChanged(data, error);
        }

        private void ReadingChangedHandler(CMAccelerometerData data, NSError error)
        {
            AccelerometerReading reading = new AccelerometerReading();
            base.IsDataValid = (error == null);
            if (base.IsDataValid)
            {
                reading.Acceleration = new Vector3((float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z);
                reading.Timestamp = DateTime.UtcNow;
                base.CurrentValue = reading;

                _eventArgs.SensorReading = base.CurrentValue;
                base.OnCurrentValueChanged(_eventArgs);
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

            base.Dispose(disposing);
        }

    }
}