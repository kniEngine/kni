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
                if (_strategy == null)
                    return null;
                return _strategy.GraphicsDevice;
            } 
        }

        /// <summary>
        /// Raised when the GraphicsResource is disposed or finalized.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        protected GraphicsResource(bool hasStrategy)
        {
            Debug.Assert(hasStrategy == true); // parent class will set the _strategy
        }

        internal GraphicsResource()
        {
            IGraphicsResourceStrategy strategy = new GraphicsResourceStrategy();
            SetResourceStrategy(strategy);
        }

        protected GraphicsResource(GraphicsDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("graphicsDevice");

            IGraphicsResourceStrategy strategy = new GraphicsResourceStrategy();
            SetResourceStrategy(strategy);
            Debug.Assert(device != null);
            ((GraphicsResourceStrategy)_strategy).SetGraphicsDevice(device.Strategy);
        }

        protected void SetResourceStrategy(IGraphicsResourceStrategy strategy)
        {
            Debug.Assert(_strategy == null);

            _strategy = strategy;
            _strategy.Disposing += (sender, e) => { OnDisposing(e); };

            ((GraphicsResourceStrategy)_strategy).ContextLost += GraphicsResourceStrategy_ContextLost;
            ((GraphicsResourceStrategy)_strategy).DeviceDisposing += GraphicsResourceStrategy_DeviceDisposing;
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
        // They might get used on multiple GraphicsDevice objects during their lifetime,
        // but only one GraphicsDevice should retain ownership.
        internal void BindGraphicsDevice(GraphicsDevice device)
        {
            if (_strategy.GraphicsDevice != device)
            {
                Debug.Assert(device != null);
                ((GraphicsResourceStrategy)_strategy).SetGraphicsDevice(device.Strategy);
            }
        }

        private void GraphicsResourceStrategy_ContextLost(object sender, EventArgs e)
        {
            GraphicsContextLost();
        }

        private void GraphicsResourceStrategy_DeviceDisposing(object sender, EventArgs e)
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

        private void OnDisposing(EventArgs e)
        {
            var handler = Disposing;
            if (handler != null)
                handler(this, e);
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
                _strategy.Disposing -= (sender, e) => { OnDisposing(e); };

                ((GraphicsResourceStrategy)_strategy).ContextLost -= GraphicsResourceStrategy_ContextLost;
                ((GraphicsResourceStrategy)_strategy).DeviceDisposing -= GraphicsResourceStrategy_DeviceDisposing;
            }

            // Remove from the global list of graphics resources
            if (_strategy.GraphicsDevice != null)
            {
                ((GraphicsResourceStrategy)_strategy).SetGraphicsDevice(null);
            }

        }


        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }
}

