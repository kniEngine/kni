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

        internal GraphicsResourceStrategy()
        {
        }


        internal void SetGraphicsDevice(GraphicsDeviceStrategy deviceStrategy)
        {
            _deviceStrategy = deviceStrategy;
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

                _deviceStrategy = null;
            }

        }

        #endregion IDisposable Members

    }
}
