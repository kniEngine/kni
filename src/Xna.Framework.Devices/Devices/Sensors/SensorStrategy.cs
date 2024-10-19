// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{
    public class SensorStrategy<TSensorReading> : IDisposable
        where TSensorReading : ISensorReading
    {
        private SensorState _state;
        private bool _isDataValid;
        private TimeSpan _reportInterval = TimeSpan.FromMilliseconds(2);
        private TSensorReading _currentReading;

        public event EventHandler<SensorReadingEventArgs<TSensorReading>> ReadingChanged;


        public virtual SensorState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public virtual bool IsDataValid
        {
            get { return _isDataValid; }
            set { _isDataValid = value; }
        }

        public virtual TimeSpan ReportInterval
        {
            get { return _reportInterval; }
            set { _reportInterval = value; }
        }

        public virtual TSensorReading CurrentReading
        {
            get { return _currentReading; }
            set { _currentReading = value; }
        }

        public SensorStrategy()
        {
        }



        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        internal
        protected virtual void OnReadingChanged(SensorReadingEventArgs<TSensorReading> eventArgs)
        {
            var handler = ReadingChanged;
            if (handler != null)
                handler(this, eventArgs);
        }

        protected SensorFailedException CreateSensorFailedException(string message)
        {
            throw new SensorFailedException(message);
        }

        #region IDisposable Members

        ~SensorStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

        #endregion
    }
}
