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
        private WeakDeviceStrategyEvents _deviceStrategyEvents;
        private readonly WeakReference _graphicsResourceRef = new WeakReference(null);

        public GraphicsDevice GraphicsDevice
        { 
            get 
            {
                if (_deviceStrategy != null)
                    return _deviceStrategy.Device;

                return null;
            }
        }

        GraphicsResource IGraphicsResourceStrategy.GraphicsResource
        {
            get { return _graphicsResourceRef.Target as GraphicsResource;}
            set { _graphicsResourceRef.Target = value; }
        }

        internal GraphicsResourceStrategy()
        {
        }

        public GraphicsResourceStrategy(GraphicsContextStrategy contextStrategy)
        {
            BindGraphicsDevice(contextStrategy.Context.DeviceStrategy);
        }

        public GraphicsResourceStrategy(GraphicsResourceStrategy source)
        {
            BindGraphicsDevice(source._deviceStrategy);
        }

        internal void BindGraphicsDevice(GraphicsDeviceStrategy deviceStrategy)
        {
            _deviceStrategy = deviceStrategy;

            _deviceStrategyEvents = new WeakDeviceStrategyEvents(this, _deviceStrategy);
        }

        private void GraphicsDeviceStrategy_ContextLost(object sender, EventArgs e)
        {
            OnContextLost(e);
            PlatformGraphicsContextLost();
        }

        private void GraphicsDeviceStrategy_Disposing(object sender, EventArgs e)
        {
            OnDeviceDisposing(e);
        }

        private void OnContextLost(EventArgs e)
        {
            if (_graphicsResourceRef.Target != null)
            {
                GraphicsResource graphicsResource = (GraphicsResource)_graphicsResourceRef.Target;
                graphicsResource.GraphicsResourceStrategy_ContextLost();
            }
        }

        private void OnDeviceDisposing(EventArgs e)
        {
            if (_graphicsResourceRef.Target != null)
            {
                GraphicsResource graphicsResource = (GraphicsResource)_graphicsResourceRef.Target;
                graphicsResource.GraphicsResourceStrategy_DeviceDisposing();
            }
        }

        internal virtual void PlatformGraphicsContextLost()
        {

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
            if (_graphicsResourceRef.Target != null)
            {
                GraphicsResource graphicsResource = (GraphicsResource)_graphicsResourceRef.Target;
                graphicsResource.OnDisposing();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_deviceStrategyEvents != null)
            {
                _deviceStrategyEvents.Dispose();
                _deviceStrategyEvents = null;
            }

            _deviceStrategy = null;
        }

        #endregion IDisposable Members


        private sealed class  WeakDeviceStrategyEvents : IDisposable
        {
            readonly WeakReference _resourceStrategyRef;
            GraphicsDeviceStrategy _deviceStrategy;

            internal WeakDeviceStrategyEvents(GraphicsResourceStrategy resourceStrategy, GraphicsDeviceStrategy deviceStrategy)
            {
                _resourceStrategyRef = new WeakReference(resourceStrategy);
                _deviceStrategy = deviceStrategy;

                _deviceStrategy.ContextLost += this.GraphicsDeviceStrategy_ContextLost;
                _deviceStrategy.Disposing += this.GraphicsDeviceStrategy_Disposing;
            }

            private void GraphicsDeviceStrategy_ContextLost(object sender, EventArgs e)
            {
                if (_resourceStrategyRef.IsAlive)
                    ((GraphicsResourceStrategy)_resourceStrategyRef.Target).GraphicsDeviceStrategy_ContextLost(sender, e);
            }

            private void GraphicsDeviceStrategy_Disposing(object sender, EventArgs e)
            {
                if (_resourceStrategyRef.IsAlive)
                    ((GraphicsResourceStrategy)_resourceStrategyRef.Target).GraphicsDeviceStrategy_Disposing(sender, e);
            }

            public void Dispose()
            {
                _deviceStrategy.ContextLost -= this.GraphicsDeviceStrategy_ContextLost;
                _deviceStrategy.Disposing -= this.GraphicsDeviceStrategy_Disposing;

                _resourceStrategyRef.Target = null;
                _deviceStrategy = null;
            }
        }

    }
}
