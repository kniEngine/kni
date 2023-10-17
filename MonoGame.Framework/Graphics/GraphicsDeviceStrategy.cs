// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsDeviceStrategy : IDisposable
    {
        GraphicsDevice _device;
        private bool _isDisposed;

        private GraphicsAdapter _graphicsAdapter;
        private readonly GraphicsProfile _graphicsProfile;
        private bool _useHalfPixelOffset;
        private PresentationParameters _presentationParameters;
        internal GraphicsCapabilities _capabilities;
        internal GraphicsContext _mainContext;

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        public readonly Dictionary<int, Effect> EffectCache = new Dictionary<int, Effect>();


        /// <summary>
        /// Raised when the GraphicsResource is disposed or finalized.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        internal event EventHandler<EventArgs> ContextLost;
        internal event EventHandler<EventArgs> DeviceReset;
        internal event EventHandler<EventArgs> DeviceResetting;

        internal event EventHandler<PresentationEventArgs> PresentationChanged;


        internal GraphicsDevice Device { get { return _device; } }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public GraphicsAdapter Adapter
        {
            get { return _graphicsAdapter; }
            internal set { _graphicsAdapter = value; }
        }

        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
        }

        public bool UseHalfPixelOffset
        {
            get { return _useHalfPixelOffset; }
        }

        public PresentationParameters PresentationParameters
        {
            get { return _presentationParameters; }
            internal set { _presentationParameters = value; }
        }

        internal GraphicsCapabilities Capabilities
        {
            get { return _capabilities; }
        }

        internal GraphicsContext MainContext { get { return _mainContext; } }

        internal GraphicsContext CurrentContext { get { return _mainContext; } }


        protected GraphicsDeviceStrategy(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(String.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            this._device = device;
            this._graphicsAdapter = adapter;
            this._graphicsProfile = graphicsProfile;
            this._useHalfPixelOffset = preferHalfPixelOffset;
            this._presentationParameters = presentationParameters;

        }

        internal int GetClampedMultiSampleCount(SurfaceFormat surfaceFormat, int multiSampleCount, int maxMultiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                int msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);

                // and clamp it to what the device can handle
                msc = Math.Min(msc, maxMultiSampleCount);

                return msc;
            }
            else return 0;
        }


        internal void OnContextLost(EventArgs e)
        {
            var handler = ContextLost;
            if (handler != null)
                handler(this, e);
        }

        internal void OnDeviceResetting(EventArgs e)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, e);
        }

        internal void OnPresentationChanged(PresentationEventArgs e)
        {
            var handler = PresentationChanged;
            if (handler != null)
                handler(this, e);
        }

        internal void OnDeviceReset(EventArgs e)
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(this, e);
        }


        public abstract void Reset();
        public abstract void Reset(PresentationParameters presentationParameters);
        public abstract void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle);
        public virtual void Present()
        {
            // reset _graphicsMetrics
            MainContext._graphicsMetrics = new GraphicsMetrics();
        }

        public abstract void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct;
                

        internal abstract GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context);

        internal T ToConcrete<T>() where T : GraphicsDeviceStrategy
        {
            return (T)this;
        }

        public abstract Assembly ConcreteAssembly { get; }
        public abstract string ResourceNameAlphaTestEffect { get; }
        public abstract string ResourceNameBasicEffect { get; }
        public abstract string ResourceNameDualTextureEffect { get; }
        public abstract string ResourceNameEnvironmentMapEffect { get; }
        public abstract string ResourceNameSkinnedEffect { get; }
        public abstract string ResourceNameSpriteEffect { get; }

        internal byte[] GetResourceStreamBytes(string resourceName)
        {
            Stream stream = ConcreteAssembly.GetManifestResourceStream(resourceName);
            byte[] bytecode = new byte[stream.Length];
            stream.Read(bytecode, 0, (int)stream.Length);
            return bytecode;
        }

        #region IDisposable Members

        ~GraphicsDeviceStrategy()
        {
            _isDisposed = true;
            OnDisposing(EventArgs.Empty);
            Dispose(false);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                OnDisposing(EventArgs.Empty);
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void OnDisposing(EventArgs e)
        {
            var handler = Disposing;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear the effect cache.
                this.EffectCache.Clear();

                if (_mainContext != null)
                {
                    _mainContext.Dispose();
                    _mainContext = null;
                }

                _device = null;
            }
        }


        #endregion IDisposable Members

    }
}
