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
    public interface IPlatformGraphicsDevice
    {
        GraphicsDeviceStrategy Strategy { get; }

        event EventHandler<PresentationEventArgs> PresentationChanged;
    }

    public abstract class GraphicsDeviceStrategy : IDisposable
    {
        GraphicsDevice _device;
        private bool _isDisposed;

        private GraphicsAdapter _graphicsAdapter;
        private readonly GraphicsProfile _graphicsProfile;
        private bool _useHalfPixelOffset;
        private PresentationParameters _presentationParameters;
        protected internal GraphicsCapabilities _capabilities;
        protected internal GraphicsContext _mainContext;

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


        public GraphicsDevice Device { get { return _device; } }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public GraphicsAdapter Adapter
        {
            get { return _graphicsAdapter; }
            protected set { _graphicsAdapter = value; }
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
            protected set { _presentationParameters = value; }
        }

        public GraphicsCapabilities Capabilities
        {
            get { return _capabilities; }
        }

        public GraphicsContext MainContext { get { return _mainContext; } }

        public GraphicsContext CurrentContext { get { return _mainContext; } }


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

        internal void Initialize(PresentationParameters presentationParameters)
        {
            // Setup
            this.PlatformSetup(presentationParameters);

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
             ((IPlatformGraphicsContext)_mainContext).Strategy._viewport = new Viewport(0, 0, this.DisplayMode.Width, this.DisplayMode.Height);

             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexConstantBuffers = new ConstantBufferCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, 16);
             ((IPlatformGraphicsContext)_mainContext).Strategy._pixelConstantBuffers = new ConstantBufferCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, 16);

             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexTextures = new TextureCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, this.Capabilities.MaxVertexTextureSlots);
             ((IPlatformGraphicsContext)_mainContext).Strategy._pixelTextures = new TextureCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, this.Capabilities.MaxTextureSlots);

             ((IPlatformGraphicsContext)_mainContext).Strategy._pixelSamplerStates = new SamplerStateCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, this.Capabilities.MaxTextureSlots);
             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexSamplerStates = new SamplerStateCollection( ((IPlatformGraphicsContext)_mainContext).Strategy, this.Capabilities.MaxVertexTextureSlots);

             ((IPlatformGraphicsContext)_mainContext).Strategy._blendStateAdditive = new BlendState(BlendState.Additive);
             ((IPlatformGraphicsContext)_mainContext).Strategy._blendStateAlphaBlend = new BlendState(BlendState.AlphaBlend);
             ((IPlatformGraphicsContext)_mainContext).Strategy._blendStateNonPremultiplied = new BlendState(BlendState.NonPremultiplied);
             ((IPlatformGraphicsContext)_mainContext).Strategy._blendStateOpaque = new BlendState(BlendState.Opaque);

             ((IPlatformGraphicsContext)_mainContext).Strategy.BlendState = BlendState.Opaque;

             ((IPlatformGraphicsContext)_mainContext).Strategy._depthStencilStateDefault = new DepthStencilState(DepthStencilState.Default);
             ((IPlatformGraphicsContext)_mainContext).Strategy._depthStencilStateDepthRead = new DepthStencilState(DepthStencilState.DepthRead);
             ((IPlatformGraphicsContext)_mainContext).Strategy._depthStencilStateNone = new DepthStencilState(DepthStencilState.None);

            _mainContext.DepthStencilState = DepthStencilState.Default;

             ((IPlatformGraphicsContext)_mainContext).Strategy._rasterizerStateCullClockwise = new RasterizerState(RasterizerState.CullClockwise);
             ((IPlatformGraphicsContext)_mainContext).Strategy._rasterizerStateCullCounterClockwise = new RasterizerState(RasterizerState.CullCounterClockwise);
             ((IPlatformGraphicsContext)_mainContext).Strategy._rasterizerStateCullNone = new RasterizerState(RasterizerState.CullNone);

             ((IPlatformGraphicsContext)_mainContext).Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Setup end

            this.PlatformInitialize();

            // Force set the default render states.
             ((IPlatformGraphicsContext)_mainContext).Strategy._blendStateDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._blendFactorDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._depthStencilStateDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._rasterizerStateDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy.BlendState = BlendState.Opaque;
             ((IPlatformGraphicsContext)_mainContext).Strategy.DepthStencilState = DepthStencilState.Default;
             ((IPlatformGraphicsContext)_mainContext).Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Force set the buffers and shaders on next ApplyState() call
             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexBuffers = new VertexBufferCollection(this.Capabilities.MaxVertexBufferSlots);
             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexBuffersDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._indexBufferDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._vertexShaderDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
             ((IPlatformGraphicsContext)_mainContext).Strategy._scissorRectangleDirty = true;
             ((IPlatformGraphicsContext)_mainContext).Strategy._scissorRectangle =  ((IPlatformGraphicsContext)_mainContext).Strategy._viewport.Bounds;

            // Set the default render target.
             ((IPlatformGraphicsContext)_mainContext).Strategy.ApplyRenderTargets(null);
        }

        protected abstract void PlatformSetup(PresentationParameters presentationParameters);
        protected abstract void PlatformInitialize();

        protected void OnContextLost(EventArgs e)
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

        protected internal void OnDeviceReset(EventArgs e)
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
            ((IPlatformGraphicsContext)MainContext).Strategy._graphicsMetrics = new GraphicsMetrics();
        }

        public abstract void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct;
                

        public abstract GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context);

        protected GraphicsContext CreateGraphicsContext()
        {
            return new GraphicsContext(this);
        }

        public T ToConcrete<T>() where T : GraphicsDeviceStrategy
        {
            return (T)this;
        }

        public abstract Assembly ConcreteAssembly { get; }

        internal const string ResourceNameAlphaTestEffect = "Resources.AlphaTestEffect.fxo";
        internal const string ResourceNameBasicEffect = "Resources.BasicEffect.fxo";
        internal const string ResourceNameDualTextureEffect = "Resources.DualTextureEffect.fxo";
        internal const string ResourceNameEnvironmentMapEffect = "Resources.EnvironmentMapEffect.fxo";
        internal const string ResourceNameSkinnedEffect = "Resources.SkinnedEffect.fxo";
        internal const string ResourceNameSpriteEffect = "Resources.SpriteEffect.fxo";

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
