// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices.Sensors;

namespace Microsoft.Xna.Platform.Devices.Sensors
{   
    public sealed class SensorService : IDisposable
        , IPlatformSensorService
    {
        private volatile static SensorService _current;
        private SensorServiceStrategy _strategy;

        public readonly static object SyncHandle = new object();

        SensorServiceStrategy IPlatformSensorService.Strategy { get { return _strategy; } }

        public static SensorService Current
        {
            get
            {
                SensorService current = _current;
                if (current != null)
                    return current;

                // Create instance
                lock(SyncHandle)
                {
                    if (_current == null)
                    {   
                        try
                        {
                            _current = new SensorService();
                        }
                        catch (Exception ex)
                        {
                            throw new ("SensorService has failed to initialize.", ex);
                        }
                    }
                    return _current;
                }
            }
        }

        internal bool IsAccelerometerSupported
        {
            get { return _strategy.PlatformIsAccelerometerSupported(); }
        }

        internal bool IsCompassSupported
        {
            get { return _strategy.PlatformIsCompassSupported(); }
        }

        private SensorService()
        {
            _strategy = new ConcreteSensorService();
        }

        public static void Suspend()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current._strategy.Suspend();
                }
            }
        }

        public static void Resume()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current._strategy.Resume();
                }
            }
        }

        public static void Shutdown()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current.Dispose();
                    _current = null;
                }
            }
        }

        #region IDisposable

        private bool isDisposed = false;
        public event EventHandler Disposing;

        ~SensorService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (isDisposed)
                    return;

                var handler = Disposing;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                _strategy.Dispose();
                _strategy = null;

                isDisposed = true;
            }
            else
            {
                if (isDisposed)
                    return;
                                
                _strategy = null;

                isDisposed = true;
            }
        }


        #endregion // IDisposable
    }
}

