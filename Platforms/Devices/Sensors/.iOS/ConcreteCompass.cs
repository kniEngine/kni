﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Devices.Sensors;
using CoreMotion;
using Foundation;

namespace Microsoft.Platform.Devices.Sensors
{
    internal class ConcreteCompass : CompassStrategy
    {
        static int _instanceCount;
        private static SensorState _state = (SensorService.Current.IsCompassSupported)
                                          ? SensorState.Initializing 
                                          : SensorState.NotSupported;
        private bool _calibrate = false;

        private static event CMDeviceMotionHandler readingChanged;

        private SensorReadingEventArgs<CompassReading> _eventArgs = new SensorReadingEventArgs<CompassReading>(default(CompassReading));


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
                    ConcreteAccelerometer._motionManager.MagnetometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
                }
            }
        }

        public override CompassReading CurrentValue
        {
            get { return base.CurrentValue; }
            set { base.CurrentValue = value; }
        }


        public ConcreteCompass()
        {
            if (!SensorService.Current.IsCompassSupported)
                throw new SensorFailedException("Failed to start compass data acquisition. No default sensor found.");

            _instanceCount++;

            readingChanged += ReadingChangedHandler;
        }

        public override void Start()
        {
            if (this.State == SensorState.Ready)
                throw new SensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");

            // For true north use CMAttitudeReferenceFrame.XTrueNorthZVertical, but be aware that it requires location service
            ConcreteAccelerometer._motionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XMagneticNorthZVertical, NSOperationQueue.CurrentQueue, MagnetometerHandler);
            _state = SensorState.Ready;
        }

        public override void Stop()
        {
            ConcreteAccelerometer._motionManager.StopDeviceMotionUpdates();
            _state = SensorState.Disabled;
        }

        private void MagnetometerHandler(CMDeviceMotion magnetometerData, NSError error)
        {
            readingChanged(magnetometerData, error);
        }

        private void ReadingChangedHandler(CMDeviceMotion data, NSError error)
        {
            CompassReading reading = new CompassReading();
            base.IsDataValid = (error == null);
            if (base.IsDataValid)
            {
                reading.MagnetometerReading = new Vector3((float)data.MagneticField.Field.Y, (float)-data.MagneticField.Field.X, (float)data.MagneticField.Field.Z);
                reading.TrueHeading = Math.Atan2(reading.MagnetometerReading.Y, reading.MagnetometerReading.X) / Math.PI * 180;
                reading.MagneticHeading = reading.TrueHeading;
                switch (data.MagneticField.Accuracy)
                {
                    case CMMagneticFieldCalibrationAccuracy.High:
                        reading.HeadingAccuracy = 5d;
                        break;
                    case CMMagneticFieldCalibrationAccuracy.Medium:
                        reading.HeadingAccuracy = 30d;
                        break;
                    case CMMagneticFieldCalibrationAccuracy.Low:
                        reading.HeadingAccuracy = 45d;
                        break;
                }

                // Send calibrate event if needed
                if (data.MagneticField.Accuracy == CMMagneticFieldCalibrationAccuracy.Uncalibrated)
                {
                    if (this._calibrate == false)
                    {
                        base.OnCalibrate(CalibrationEventArgs.Empty);
                    }
                    this._calibrate = true;
                }
                else if (this._calibrate == true)
                    this._calibrate = false;

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