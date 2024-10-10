using System;

using Microsoft.Xna.Framework;

using CoreMotion;
using Foundation;

namespace Microsoft.Devices.Sensors
{
    public sealed class Compass : SensorBase<CompassReading>
    {
        const int MaxSensorCount = 10;

        private bool _isDisposed;
        private bool _isDataValid;

        static int _instanceCount;
        private static bool _started = false;
        private static SensorState _state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;

        private bool _calibrate = false;

        public event EventHandler<CalibrationEventArgs> Calibrate;

        public static bool IsSupported
        {
            get { return _motionManager.DeviceMotionAvailable; }
        }
        public SensorState State
        {
            get { return _state; }
        }

        protected override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public override bool IsDataValid
        {
            get { return _isDataValid; }
        }

        private static event CMDeviceMotionHandler readingChanged;

        public Compass()
        {
            if (!IsSupported)
                throw new SensorFailedException("Failed to start compass data acquisition. No default sensor found.");
            else if (_instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Compass class per application has been exceeded.");

            ++_instanceCount;

            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;
        }

        public override void Start()
        {
            if (_started == false)
            {
                // For true north use CMAttitudeReferenceFrame.XTrueNorthZVertical, but be aware that it requires location service
                _motionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XMagneticNorthZVertical, NSOperationQueue.CurrentQueue, MagnetometerHandler);
                _started = true;
                _state = SensorState.Ready;
            }
            else
                throw new SensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");
        }

        public override void Stop()
        {
            _motionManager.StopDeviceMotionUpdates();
            _started = false;
            _state = SensorState.Disabled;
        }

        private void MagnetometerHandler(CMDeviceMotion magnetometerData, NSError error)
        {
            readingChanged(magnetometerData, error);
        }

        private void ReadingChangedHandler(CMDeviceMotion data, NSError error)
        {
            CompassReading reading = new CompassReading();
            _isDataValid = (error == null);
            if (_isDataValid)
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
                        var handler = Calibrate;
                        if (handler != null)
                            handler(this, CalibrationEventArgs.Empty);
                    }
                    this._calibrate = true;
                }
                else if (this._calibrate == true)
                    this._calibrate = false;

                reading.Timestamp = DateTime.UtcNow;
                this.CurrentValue = reading;
            }
        }

        private void UpdateInterval(object sender, EventArgs args)
        {
            _motionManager.MagnetometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_started)
                        Stop();
                    --_instanceCount;
                }

                _isDisposed = true;
                //base.Dispose(disposing);
            }
        }
    }
}

