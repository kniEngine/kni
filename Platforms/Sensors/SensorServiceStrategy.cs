// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Devices.Sensors;

namespace Microsoft.Xna.Platform.Input.Sensors
{
    public interface IPlatformSensorService
    {
        SensorServiceStrategy Strategy { get; }
    }

    abstract public class SensorServiceStrategy : IDisposable
    {
        public abstract void Suspend();
        public abstract void Resume();
        public abstract bool PlatformIsAccelerometerSupported();
        public abstract bool PlatformIsCompassSupported();

        public T ToConcrete<T>() where T : SensorServiceStrategy
        {
            return (T)this;
        }


        #region IDisposable
        ~SensorServiceStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

}
