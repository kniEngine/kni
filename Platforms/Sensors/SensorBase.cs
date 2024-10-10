// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Devices.Sensors
{
    public abstract class SensorBase<TSensorReading> : IDisposable
        where TSensorReading : ISensorReading
    {
#if IOS || TVOS
        [CLSCompliant(false)]
        protected static readonly CoreMotion.CMMotionManager _motionManager = new CoreMotion.CMMotionManager();
#endif
        bool _disposed;
        private TimeSpan _timeBetweenUpdates;
        private TSensorReading _currentValue;
        private SensorReadingEventArgs<TSensorReading> _eventArgs = new SensorReadingEventArgs<TSensorReading>(default(TSensorReading));

        public TSensorReading CurrentValue 
        {
            get { return _currentValue; }
            protected set
            {
                _currentValue = value;

                var handler = CurrentValueChanged;
                if (handler != null)
                {
                    _eventArgs.SensorReading = value;
                    handler(this, _eventArgs);
                }
            }
        }
        public bool IsDataValid { get; protected set; }
        public TimeSpan TimeBetweenUpdates
        {
            get { return this._timeBetweenUpdates; }
            set
            {
                if (this._timeBetweenUpdates != value)
                {
                    this._timeBetweenUpdates = value;
                    var handler = TimeBetweenUpdatesChanged;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged;
        protected event EventHandler<EventArgs> TimeBetweenUpdatesChanged;
        protected bool IsDisposed { get { return _disposed; } }

        public SensorBase()
        {
            this.TimeBetweenUpdates = TimeSpan.FromMilliseconds(2);
        }

        public abstract void Start();

        public abstract void Stop();

        #region IDisposable

        ~SensorBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Derived classes override this method to dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if unmanaged resources are to be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        #endregion IDisposable
    }
}

