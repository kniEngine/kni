// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Devices.Sensors;

namespace Microsoft.Xna.Platform.Input.Sensors
{
    public class SensorStrategy<TSensorReading> : IDisposable
        where TSensorReading : ISensorReading
    {
        private SensorState _state;
        private bool _isDataValid;
        private TimeSpan _timeBetweenUpdates = TimeSpan.FromMilliseconds(2);
        private TSensorReading _currentValue;


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

        public virtual TimeSpan TimeBetweenUpdates
        {
            get { return _timeBetweenUpdates; }
            set { _timeBetweenUpdates = value; }
        }

        public virtual TSensorReading CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = value; }
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
