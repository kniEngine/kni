// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class GraphicsResourceStrategy : IGraphicsResourceStrategy
    {
        private GraphicsDeviceStrategy _deviceStrategy;

        public GraphicsDevice GraphicsDevice
        { 
            get 
            {
                if (_deviceStrategy == null)
                    return null;
                return _deviceStrategy.Device;
            }
        }

        public event EventHandler<EventArgs> Disposing;

        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<EventArgs> DeviceDisposing;

        internal GraphicsResourceStrategy()
        {
        }


        internal void SetGraphicsDevice(GraphicsDeviceStrategy deviceStrategy)
        {
            if (_deviceStrategy == deviceStrategy)
                return;

            if (_deviceStrategy != null)
            {
                _deviceStrategy.DeviceResetting -= GraphicsDeviceStrategy_DeviceResetting;
                _deviceStrategy.Disposing -= GraphicsDeviceStrategy_Disposing;
            }

            _deviceStrategy = deviceStrategy;

            if (_deviceStrategy != null)
            {
                _deviceStrategy.DeviceResetting += GraphicsDeviceStrategy_DeviceResetting;
                _deviceStrategy.Disposing += GraphicsDeviceStrategy_Disposing;
            }
        }

        private void GraphicsDeviceStrategy_DeviceResetting(object sender, EventArgs e)
        {
            OnDeviceResetting(e);
        }

        private void GraphicsDeviceStrategy_Disposing(object sender, EventArgs e)
        {
            OnDeviceDisposing(e);
        }

        private void OnDeviceResetting(EventArgs e)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, e);
        }

        private void OnDeviceDisposing(EventArgs e)
        {
            var handler = DeviceDisposing;
            if (handler != null)
                handler(this, e);
        }


        #region IDisposable Members

        ~GraphicsResourceStrategy()
        {
            OnDisposing(EventArgs.Empty);
            Dispose(false);
        }

        public void Dispose()
        {
            OnDisposing(EventArgs.Empty);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void OnDisposing(EventArgs e)
        {
            var handler = Disposing;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SetGraphicsDevice(null);
                _deviceStrategy = null;
            }

        }

        #endregion IDisposable Members

    }
}
