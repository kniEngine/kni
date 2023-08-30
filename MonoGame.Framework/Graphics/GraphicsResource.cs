// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;


namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class GraphicsResource : IDisposable
    {
        private bool _isDisposed;

        // The GraphicsDevice property should only be accessed in Dispose(bool) if
        // the disposing parameter is true.
        // If disposing is false, the GraphicsDevice may or may not be disposed yet.
        private GraphicsDevice _graphicsDevice;

        private WeakReference _selfReference;

        public string Name { get; set; }

        public Object Tag { get; set; }

        public bool IsDisposed { get { return _isDisposed; } }

        public GraphicsDevice GraphicsDevice { get { return _graphicsDevice; } }

        public event EventHandler<EventArgs> Disposing;


        internal GraphicsResource()
        {

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
        internal protected virtual void GraphicsDeviceResetting()
        {

        }

        // VertexDeclaration and State objects need to be late-bound to the GraphicsDevice.
        // They might get used on multiple GraphicsDevice objects during their lifetime, 
        // but only one GraphicsDevice should retain ownership.
        internal void BindGraphicsDevice(GraphicsDevice device)
        {
            if (_graphicsDevice != device)
            {
                if (_graphicsDevice != null)
                {
                    _graphicsDevice.Strategy.RemoveResourceReference(_selfReference);
                    _selfReference = null;
                }

                SetGraphicsDevice(device);
            }
        }

        internal void SetGraphicsDevice(GraphicsDevice device)
        {
            Debug.Assert(device != null);

            _graphicsDevice = device;

            _selfReference = new WeakReference(this);
            _graphicsDevice.Strategy.AddResourceReference(_selfReference);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method that derived classes should override to implement disposing of managed and native resources.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        /// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Do not trigger the event if called from the finalizer
                if (disposing)
                {
                    var handler = Disposing;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }

                if (disposing)
                {
                }

                // Remove from the global list of graphics resources
                if (_graphicsDevice != null)
                    _graphicsDevice.Strategy.RemoveResourceReference(_selfReference);

                _selfReference = null;
                _graphicsDevice = null;
                _isDisposed = true;
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }
}

