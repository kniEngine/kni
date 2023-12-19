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

        public DisplayMode DisplayMode
        {
            get { return _graphicsAdapter.CurrentDisplayMode; }
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

        internal void Initialize()
        {
            // Setup
            this.PlatformSetup();

#if DEBUG
            if (this.DisplayMode == null)
            {
                throw new Exception(
                    "Unable to determine the current display mode.  This can indicate that the " +
                    "game is not configured to be HiDPI aware under Windows 10 or later.  See " +
                    "https://github.com/MonoGame/MonoGame/issues/5040 for more information.");
            }
#endif

            // Initialize the main viewport
            _mainContext.Strategy._viewport = new Viewport(0, 0, this.DisplayMode.Width, this.DisplayMode.Height);

            _mainContext.Strategy._vertexConstantBuffers = new ConstantBufferCollection(_mainContext.Strategy, 16);
            _mainContext.Strategy._pixelConstantBuffers = new ConstantBufferCollection(_mainContext.Strategy, 16);

            _mainContext.Strategy._vertexTextures = new TextureCollection(_mainContext.Strategy, this.Capabilities.MaxVertexTextureSlots);
            _mainContext.Strategy._pixelTextures = new TextureCollection(_mainContext.Strategy, this.Capabilities.MaxTextureSlots);

            _mainContext.Strategy._pixelSamplerStates = new SamplerStateCollection(_mainContext.Strategy, this.Capabilities.MaxTextureSlots);
            _mainContext.Strategy._vertexSamplerStates = new SamplerStateCollection(_mainContext.Strategy, this.Capabilities.MaxVertexTextureSlots);

            _mainContext.Strategy._blendStateAdditive = new BlendState(BlendState.Additive);
            _mainContext.Strategy._blendStateAlphaBlend = new BlendState(BlendState.AlphaBlend);
            _mainContext.Strategy._blendStateNonPremultiplied = new BlendState(BlendState.NonPremultiplied);
            _mainContext.Strategy._blendStateOpaque = new BlendState(BlendState.Opaque);

            _mainContext.Strategy.BlendState = BlendState.Opaque;

            _mainContext.Strategy._depthStencilStateDefault = new DepthStencilState(DepthStencilState.Default);
            _mainContext.Strategy._depthStencilStateDepthRead = new DepthStencilState(DepthStencilState.DepthRead);
            _mainContext.Strategy._depthStencilStateNone = new DepthStencilState(DepthStencilState.None);

            _mainContext.DepthStencilState = DepthStencilState.Default;

            _mainContext.Strategy._rasterizerStateCullClockwise = new RasterizerState(RasterizerState.CullClockwise);
            _mainContext.Strategy._rasterizerStateCullCounterClockwise = new RasterizerState(RasterizerState.CullCounterClockwise);
            _mainContext.Strategy._rasterizerStateCullNone = new RasterizerState(RasterizerState.CullNone);

            _mainContext.Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Setup end

            this.PlatformInitialize();

            // Force set the default render states.
            _mainContext.Strategy._blendStateDirty = true;
            _mainContext.Strategy._blendFactorDirty = true;
            _mainContext.Strategy._depthStencilStateDirty = true;
            _mainContext.Strategy._rasterizerStateDirty = true;
            _mainContext.Strategy.BlendState = BlendState.Opaque;
            _mainContext.Strategy.DepthStencilState = DepthStencilState.Default;
            _mainContext.Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Force set the buffers and shaders on next ApplyState() call
            _mainContext.Strategy._vertexBuffers = new VertexBufferBindings(this.Capabilities.MaxVertexBufferSlots);
            _mainContext.Strategy._vertexBuffersDirty = true;
            _mainContext.Strategy._indexBufferDirty = true;
            _mainContext.Strategy._vertexShaderDirty = true;
            _mainContext.Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
            _mainContext.Strategy._scissorRectangleDirty = true;
            _mainContext.Strategy._scissorRectangle = _mainContext.Strategy._viewport.Bounds;

            // Set the default render target.
            _mainContext.ApplyRenderTargets(null);
        }

        protected abstract void PlatformSetup();
        protected abstract void PlatformInitialize();

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
