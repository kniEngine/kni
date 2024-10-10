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

        public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged;

        public abstract TSensorReading CurrentValue { get; }

        public abstract bool IsDataValid { get; }

        public abstract TimeSpan TimeBetweenUpdates { get; set; }

        protected abstract bool IsDisposed { get; }

        public SensorBase()
        {
        }

        public abstract void Start();

        public abstract void Stop();

        protected virtual void OnCurrentValueChanged(SensorReadingEventArgs<TSensorReading> eventArgs)
        {
            var handler = CurrentValueChanged;
            if (handler != null)
                handler(this, eventArgs);
        }

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
        protected abstract void Dispose(bool disposing);

        #endregion IDisposable
    }
}

