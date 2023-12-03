// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class GraphicsResource : IDisposable
    {
        private IGraphicsResourceStrategy _strategy;

        private bool _isDisposed;

        public string Name { get; set; }

        public Object Tag { get; set; }

        public bool IsDisposed { get { return _isDisposed; } }

        public GraphicsDevice GraphicsDevice
        { 
            get 
            {
                if (_strategy != null)
                    return _strategy.GraphicsDevice;

                return null;
            } 
        }

        /// <summary>
        /// Raised when the GraphicsResource is disposed or finalized.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        protected GraphicsResource()
        {
            // parent class will set the _strategy
        }

        protected GraphicsResource(GraphicsDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("graphicsDevice");

            IGraphicsResourceStrategy strategy = new GraphicsResourceStrategy();
            SetResourceStrategy(strategy);
            Debug.Assert(device != null);
            ((GraphicsResourceStrategy)_strategy).BindGraphicsDevice(device.Strategy);
        }

        protected void SetResourceStrategy(IGraphicsResourceStrategy strategy)
        {
            Debug.Assert(_strategy == null);

            _strategy = strategy;
            _strategy.GraphicsResource = this;
        }

        ~GraphicsResource()
        {
             Dispose(false);
        }



        /// <summary>
        /// Called before the device is reset. Allows graphics resources to 
        /// invalidate their state so they can be recreated after the device reset.
        /// Warning: This may be called after a call to Dispose() up until
        /// the resource is garbage collected.
        /// </summary>
        internal protected virtual void GraphicsContextLost()
        {

        }

        // State objects need to be late-bound to the GraphicsDevice.
        // Oonly one GraphicsDevice should retain ownership.
        internal void BindGraphicsDevice(GraphicsDeviceStrategy deviceStrategy)
        {
            ((GraphicsResourceStrategy)_strategy).BindGraphicsDevice(deviceStrategy);
        }

        internal void GraphicsResourceStrategy_ContextLost()
        {
            GraphicsContextLost();
        }

        internal void GraphicsResourceStrategy_DeviceDisposing()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                _isDisposed = true;
            }
        }

        internal void OnDisposing()
        {
            var handler = Disposing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// The method that derived classes should override to implement disposing of managed and native resources.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        /// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                _strategy.Dispose();
                _strategy.GraphicsResource = null;
            }
        }


        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }
}

