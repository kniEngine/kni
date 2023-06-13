// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice : IDisposable
    {
        /// <summary>
        /// Indicates if DX9 style pixel addressing or current standard
        /// pixel addressing should be used. This flag is set to
        /// <c>false</c> by default. If `UseHalfPixelOffset` is
        /// `true` you have to add half-pixel offset to a Projection matrix.
        /// See also <see cref="GraphicsDeviceManager.PreferHalfPixelOffset"/>.
        /// </summary>
        /// <remarks>
        /// XNA uses DirectX9 for its graphics. DirectX9 interprets UV
        /// coordinates differently from other graphics API's. This is
        /// typically referred to as the half-pixel offset. MonoGame
        /// replicates XNA behavior if this flag is set to <c>true</c>.
        /// </remarks>
        public bool UseHalfPixelOffset { get; private set; }

        private bool _isDisposed;

        private GraphicsContext _mainContext;

        private Color _discardColor = new Color(68, 34, 136, 255);

        private Viewport _viewport;

        internal GraphicsContext CurrentContext { get { return _mainContext; } }

        internal GraphicsCapabilities GraphicsCapabilities { get; private set; }


        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        internal readonly Dictionary<int, Effect> EffectCache = new Dictionary<int, Effect>();

        // Resources may be added to and removed from the list from many threads.
        private readonly object _resourcesLock = new object();

        // Use WeakReference for the global resources list as we do not know when a resource
        // may be disposed and collected. We do not want to prevent a resource from being
        // collected by holding a strong reference to it in this list.
        private readonly List<WeakReference> _resources = new List<WeakReference>();

        // TODO Graphics Device events need implementing
        public event EventHandler<EventArgs> DeviceLost;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
        public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;
        public event EventHandler<EventArgs> Disposing;

        internal event EventHandler<PresentationEventArgs> PresentationChanged;

        public bool IsDisposed
        { 
            get { return _isDisposed; } 
        }

        public bool IsContentLost
        {
            get
            {
                // We will just return IsDisposed for now
                // as that is the only case I can see for now
                return IsDisposed;
            }
        }

        internal bool IsRenderTargetBound
        {
            get { return _mainContext.Strategy._currentRenderTargetCount > 0; }
        }

        public GraphicsAdapter Adapter
        {
            get;
            private set;
        }

        /// <summary>
        /// The rendering information for debugging and profiling.
        /// The metrics are reset every frame after draw within <see cref="GraphicsDevice.Present"/>. 
        /// </summary>
        public GraphicsMetrics Metrics
        {
            get { return CurrentContext.Metrics; }
            set { CurrentContext.Metrics = value; }
        }

        private GraphicsDebug _graphicsDebug;

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug { get { return _graphicsDebug; } set { _graphicsDebug = value; } }

        internal GraphicsDevice()
		{
            PresentationParameters = new PresentationParameters();
            PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(String.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            Adapter = adapter;
            PresentationParameters = presentationParameters;
            _graphicsProfile = graphicsProfile;

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="preferHalfPixelOffset"> Indicates if DX9 style pixel addressing or current standard pixel addressing should be used. This value is passed to <see cref="GraphicsDevice.UseHalfPixelOffset"/></param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(String.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

#if DIRECTX
            // TODO we need to figure out how to inject the half pixel offset into DX shaders
            preferHalfPixelOffset = false;
#endif

            Adapter = adapter;
            _graphicsProfile = graphicsProfile;
            UseHalfPixelOffset = preferHalfPixelOffset;
            PresentationParameters = presentationParameters;

            Initialize();
        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        internal int GetClampedMultisampleCount(int multiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                var msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);
                // and clamp it to what the device can handle
                if (msc > GraphicsCapabilities.MaxMultiSampleCount)
                    msc = GraphicsCapabilities.MaxMultiSampleCount;

                return msc;
            }
            else return 0;
        }

        private void Initialize()
        {
            // Setup

#if DEBUG
            if (DisplayMode == null)
            {
                throw new Exception(
                    "Unable to determine the current display mode.  This can indicate that the " +
                    "game is not configured to be HiDPI aware under Windows 10 or later.  See " +
                    "https://github.com/MonoGame/MonoGame/issues/5040 for more information.");
            }
#endif

            // Initialize the main viewport
            _viewport = new Viewport(0, 0, DisplayMode.Width, DisplayMode.Height);

            PlatformSetup();

            _mainContext.Strategy._textures = new TextureCollection(this, ShaderStage.Pixel, GraphicsCapabilities.MaxTextureSlots);
            _mainContext.Strategy._vertexTextures = new TextureCollection(this, ShaderStage.Vertex, GraphicsCapabilities.MaxVertexTextureSlots);

            _mainContext.Strategy._samplerStates = new SamplerStateCollection(this, ShaderStage.Pixel, GraphicsCapabilities.MaxTextureSlots);
            _mainContext.Strategy._vertexSamplerStates = new SamplerStateCollection(this, ShaderStage.Vertex, GraphicsCapabilities.MaxVertexTextureSlots);

            _mainContext.Strategy._blendStateAdditive = BlendState.Additive.Clone();
            _mainContext.Strategy._blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
            _mainContext.Strategy._blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
            _mainContext.Strategy._blendStateOpaque = BlendState.Opaque.Clone();

            BlendState = BlendState.Opaque;

            _mainContext.Strategy._depthStencilStateDefault = DepthStencilState.Default.Clone();
            _mainContext.Strategy._depthStencilStateDepthRead = DepthStencilState.DepthRead.Clone();
            _mainContext.Strategy._depthStencilStateNone = DepthStencilState.None.Clone();

            DepthStencilState = DepthStencilState.Default;

            _mainContext.Strategy._rasterizerStateCullClockwise = RasterizerState.CullClockwise.Clone();
            _mainContext.Strategy._rasterizerStateCullCounterClockwise = RasterizerState.CullCounterClockwise.Clone();
            _mainContext.Strategy._rasterizerStateCullNone = RasterizerState.CullNone.Clone();

            RasterizerState = RasterizerState.CullCounterClockwise;

            // Setup end

            PlatformInitialize();

            // Force set the default render states.
            _mainContext.Strategy._blendStateDirty = true;
            _mainContext.Strategy._blendFactorDirty = true;
            _mainContext.Strategy._depthStencilStateDirty = true;
            _mainContext.Strategy._rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Clear constant buffers
            _mainContext.Strategy._vertexConstantBuffers.Clear();
            _mainContext.Strategy._pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _mainContext.Strategy._vertexBuffers = new VertexBufferBindings(GraphicsCapabilities.MaxVertexBufferSlots);
            _mainContext.Strategy._vertexBuffersDirty = true;
            _mainContext.Strategy._indexBufferDirty = true;
            _mainContext.Strategy._vertexShaderDirty = true;
            _mainContext.Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
            _mainContext.Strategy._scissorRectangleDirty = true;
            _mainContext.Strategy._scissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets(null);
        }

        public Rectangle ScissorRectangle
        {
            get { return _mainContext.Strategy._scissorRectangle; }
            set
            {
                if (_mainContext.Strategy._scissorRectangle == value)
                    return;

                _mainContext.Strategy._scissorRectangle = value;
                _mainContext.Strategy._scissorRectangleDirty = true;
            }
        }

        public Viewport Viewport
        {
            get { return _viewport; }
            set
            {
                _viewport = value;
                PlatformApplyViewport();
            }
        }

        public BlendState BlendState
        {
			get { return _mainContext.Strategy._blendState; }
			set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_mainContext.Strategy._blendState == value)
                    return;

                _mainContext.Strategy._blendState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newBlendState = _mainContext.Strategy._blendState;
                if (ReferenceEquals(_mainContext.Strategy._blendState, BlendState.Additive))
                    newBlendState = _mainContext.Strategy._blendStateAdditive;
                else if (ReferenceEquals(_mainContext.Strategy._blendState, BlendState.AlphaBlend))
                    newBlendState = _mainContext.Strategy._blendStateAlphaBlend;
                else if (ReferenceEquals(_mainContext.Strategy._blendState, BlendState.NonPremultiplied))
                    newBlendState = _mainContext.Strategy._blendStateNonPremultiplied;
                else if (ReferenceEquals(_mainContext.Strategy._blendState, BlendState.Opaque))
                    newBlendState = _mainContext.Strategy._blendStateOpaque;

                if (newBlendState.IndependentBlendEnable && !GraphicsCapabilities.SupportsSeparateBlendStates)
                    throw new PlatformNotSupportedException("Independent blend states requires at least OpenGL 4.0 or GL_ARB_draw_buffers_blend. Try upgrading your graphics drivers.");

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(this);

                _mainContext.Strategy._actualBlendState = newBlendState;

                BlendFactor = _mainContext.Strategy._actualBlendState.BlendFactor;

                _mainContext.Strategy._blendStateDirty = true;
            }
		}

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// When only changing BlendFactor, use this rather than <see cref="Graphics.BlendState.BlendFactor"/> to
        /// only update BlendFactor so the whole BlendState does not have to be updated.
        /// </remarks>
        public Color BlendFactor
        {
            get { return _mainContext.Strategy._blendFactor; }
            set
            {
                if (_mainContext.Strategy._blendFactor == value)
                    return;
                _mainContext.Strategy._blendFactor = value;
                _mainContext.Strategy._blendFactorDirty = true;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get { return _mainContext.Strategy._depthStencilState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_mainContext.Strategy._depthStencilState == value)
                    return;

                _mainContext.Strategy._depthStencilState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newDepthStencilState = _mainContext.Strategy._depthStencilState;
                if (ReferenceEquals(_mainContext.Strategy._depthStencilState, DepthStencilState.Default))
                    newDepthStencilState = _mainContext.Strategy._depthStencilStateDefault;
                else if (ReferenceEquals(_mainContext.Strategy._depthStencilState, DepthStencilState.DepthRead))
                    newDepthStencilState = _mainContext.Strategy._depthStencilStateDepthRead;
                else if (ReferenceEquals(_mainContext.Strategy._depthStencilState, DepthStencilState.None))
                    newDepthStencilState = _mainContext.Strategy._depthStencilStateNone;

                newDepthStencilState.BindToGraphicsDevice(this);

                _mainContext.Strategy._actualDepthStencilState = newDepthStencilState;

                _mainContext.Strategy._depthStencilStateDirty = true;
            }
        }

        public RasterizerState RasterizerState
        {
            get { return _mainContext.Strategy._rasterizerState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_mainContext.Strategy._rasterizerState == value)
                    return;

                if (!value.DepthClipEnable && !GraphicsCapabilities.SupportsDepthClamp)
                    throw new InvalidOperationException("Cannot set RasterizerState.DepthClipEnable to false on this graphics device");

                _mainContext.Strategy._rasterizerState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newRasterizerState = _mainContext.Strategy._rasterizerState;
                if (ReferenceEquals(_mainContext.Strategy._rasterizerState, RasterizerState.CullClockwise))
                    newRasterizerState = _mainContext.Strategy._rasterizerStateCullClockwise;
                else if (ReferenceEquals(_mainContext.Strategy._rasterizerState, RasterizerState.CullCounterClockwise))
                    newRasterizerState = _mainContext.Strategy._rasterizerStateCullCounterClockwise;
                else if (ReferenceEquals(_mainContext.Strategy._rasterizerState, RasterizerState.CullNone))
                    newRasterizerState = _mainContext.Strategy._rasterizerStateCullNone;

                newRasterizerState.BindToGraphicsDevice(this);

                _mainContext.Strategy._actualRasterizerState = newRasterizerState;

                _mainContext.Strategy._rasterizerStateDirty = true;
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _mainContext.Strategy._samplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return _mainContext.Strategy._vertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return _mainContext.Strategy._textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return _mainContext.Strategy._vertexTextures; }
        }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public Color DiscardColor
        {
			get { return _discardColor; }
			set { _discardColor = value; }
		}

        public void Clear(Color color)
        {
            var options = ClearOptions.Target;
            options |= ClearOptions.DepthBuffer;
            options |= ClearOptions.Stencil;
            PlatformClear(options, color.ToVector4(), _viewport.MaxDepth, 0);

            unchecked { CurrentContext._graphicsMetrics._clearCount++; }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            PlatformClear(options, color.ToVector4(), depth, stencil);

            unchecked { CurrentContext._graphicsMetrics._clearCount++; }
        }

		public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            PlatformClear(options, color, depth, stencil);

            unchecked { CurrentContext._graphicsMetrics._clearCount++; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose of all remaining graphics resources before disposing of the graphics device
                    lock (_resourcesLock)
                    {
                        foreach (var resource in _resources.ToArray())
                        {
                            var target = resource.Target as IDisposable;
                            if (target != null)
                                target.Dispose();
                        }
                        _resources.Clear();
                    }

                    // Clear the effect cache.
                    EffectCache.Clear();

                    _mainContext.Strategy._blendState = null;
                    _mainContext.Strategy._actualBlendState = null;
                    _mainContext.Strategy._blendStateAdditive.Dispose();
                    _mainContext.Strategy._blendStateAlphaBlend.Dispose();
                    _mainContext.Strategy._blendStateNonPremultiplied.Dispose();
                    _mainContext.Strategy._blendStateOpaque.Dispose();

                    _mainContext.Strategy._depthStencilState = null;
                    _mainContext.Strategy._actualDepthStencilState = null;
                    _mainContext.Strategy._depthStencilStateDefault.Dispose();
                    _mainContext.Strategy._depthStencilStateDepthRead.Dispose();
                    _mainContext.Strategy._depthStencilStateNone.Dispose();

                    _mainContext.Strategy._rasterizerState = null;
                    _mainContext.Strategy._actualRasterizerState = null;
                    _mainContext.Strategy._rasterizerStateCullClockwise.Dispose();
                    _mainContext.Strategy._rasterizerStateCullCounterClockwise.Dispose();
                    _mainContext.Strategy._rasterizerStateCullNone.Dispose();

                    PlatformDispose();
                }

                _isDisposed = true;

                var handler = Disposing;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }

        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (IsRenderTargetBound)
                throw new InvalidOperationException("Cannot call Present when a render target is active.");

            // reset _graphicsMetrics
            CurrentContext._graphicsMetrics = new GraphicsMetrics();

            PlatformPresent();
        }

        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        partial void PlatformReset();

        public void Reset()
        {
            PlatformReset();

            var deviceResettingHandler = DeviceResetting;
            if (deviceResettingHandler != null)
                deviceResettingHandler(this, EventArgs.Empty);

            // Update the back buffer.
            OnPresentationChanged();
            
            var presentationChangedHandler = PresentationChanged;
            if (presentationChangedHandler != null)
                presentationChangedHandler(this, new PresentationEventArgs(PresentationParameters));

            var deviceResetHandler = DeviceReset;
            if (deviceResetHandler != null)
                deviceResetHandler(this, EventArgs.Empty);
       }

        public void Reset(PresentationParameters presentationParameters)
        {
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            PresentationParameters = presentationParameters;
            Reset();
        }

        public DisplayMode DisplayMode
        {
            get { return Adapter.CurrentDisplayMode; }
        }

        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get { return GraphicsDeviceStatus.Normal; }
        }

        public PresentationParameters PresentationParameters
        {
            get;
            private set;
        }

        private readonly GraphicsProfile _graphicsProfile;
        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
        }


        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _mainContext.Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(_mainContext.Strategy._singleRenderTargetBinding);
            }
        }

        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _mainContext.Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(_mainContext.Strategy._singleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!GraphicsCapabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _mainContext.Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_mainContext.Strategy._singleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _mainContext.Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_mainContext.Strategy._singleRenderTargetBinding);
            }
        }

		public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
		{
            // Avoid having to check for null and zero length.
            var renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                {
                    renderTargets = null;
                }
            }

            if (this.GraphicsProfile == GraphicsProfile.Reach && renderTargetCount > 1)
                throw new NotSupportedException("Reach profile supports a maximum of 1 simultaneous rendertargets");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && renderTargetCount > 4)
                throw new NotSupportedException("HiDef profile supports a maximum of 4 simultaneous rendertargets");
            if (renderTargetCount > 8)
                throw new NotSupportedException("Current profile supports a maximum of 8 simultaneous rendertargets");

            // Try to early out if the current and new bindings are equal.
            if (_mainContext.Strategy._currentRenderTargetCount == renderTargetCount)
            {
                var isEqual = true;
                for (var i = 0; i < _mainContext.Strategy._currentRenderTargetCount; i++)
                {
                    if (_mainContext.Strategy._currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        _mainContext.Strategy._currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            ApplyRenderTargets(renderTargets);

            if (renderTargetCount == 0)
            {
                unchecked { CurrentContext._graphicsMetrics._targetCount++; }
            }
            else
            {
                unchecked { CurrentContext._graphicsMetrics._targetCount += renderTargetCount; }
            }
        }

        public int RenderTargetCount
        {
            get { return _mainContext.Strategy._currentRenderTargetCount; }
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            var clearTarget = false;

            PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(_mainContext.Strategy._currentRenderTargetBindings, 0, _mainContext.Strategy._currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                _mainContext.Strategy._currentRenderTargetCount = 0;

                PlatformApplyDefaultRenderTarget();
                clearTarget = PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
            }
			else
			{
                // Copy the new bindings.
                Array.Copy(renderTargets, _mainContext.Strategy._currentRenderTargetBindings, renderTargets.Length);
                _mainContext.Strategy._currentRenderTargetCount = renderTargets.Length;

                var renderTarget = PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            if (clearTarget)
                Clear(DiscardColor);
        }

		public RenderTargetBinding[] GetRenderTargets()
		{
            // Return a correctly sized copy our internal array.
            var bindings = new RenderTargetBinding[_mainContext.Strategy._currentRenderTargetCount];
            Array.Copy(_mainContext.Strategy._currentRenderTargetBindings, bindings, _mainContext.Strategy._currentRenderTargetCount);
            return bindings;
		}

        public void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            Debug.Assert(bindings.Length == _mainContext.Strategy._currentRenderTargetCount, "Invalid outTargets array length!");
            Array.Copy(_mainContext.Strategy._currentRenderTargetBindings, bindings, _mainContext.Strategy._currentRenderTargetCount);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            if (vertexBuffer != null)
            {
                _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Set(vertexBuffer, 0);
            }
            else
            {
                _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Clear();
            }
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            if (vertexBuffer != null)
            {
                if (0 >= vertexOffset && vertexOffset < vertexBuffer.VertexCount)
                {
                    _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Set(vertexBuffer, vertexOffset);
                }
                else
                    throw new ArgumentOutOfRangeException("vertexOffset");
            }
            else
            {
                if (vertexOffset == 0)
                {
                    _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Clear();
                }
                else
                    throw new ArgumentOutOfRangeException("vertexOffset");
            }
        }

        public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
        {
            if (vertexBuffers != null && vertexBuffers.Length > 0)
            {
                if (vertexBuffers.Length <= GraphicsCapabilities.MaxVertexBufferSlots)
                {
                    _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Set(vertexBuffers);
                }
                else
                {
                    var message = string.Format("Max number of vertex buffers is {0}.", GraphicsCapabilities.MaxVertexBufferSlots);
                    throw new ArgumentOutOfRangeException("vertexBuffers", message);
                }
            }
            else
            {
                _mainContext.Strategy._vertexBuffersDirty |= _mainContext.Strategy._vertexBuffers.Clear();
            }
        }

        public IndexBuffer Indices
        {
            get { return _mainContext.Strategy._indexBuffer; }
            set
            {
                if (_mainContext.Strategy._indexBuffer == value)
                    return;

                _mainContext.Strategy._indexBuffer = value;
                _mainContext.Strategy._indexBufferDirty = true;
            }
        }

        internal Shader VertexShader
        {
            get { return _mainContext.Strategy._vertexShader; }

            set
            {
                if (_mainContext.Strategy._vertexShader == value)
                    return;

                _mainContext.Strategy._vertexShader = value;
                _mainContext.Strategy._vertexConstantBuffers.Clear();
                _mainContext.Strategy._vertexShaderDirty = true;
            }
        }

        internal Shader PixelShader
        {
            get { return _mainContext.Strategy._pixelShader; }

            set
            {
                if (_mainContext.Strategy._pixelShader == value)
                    return;

                _mainContext.Strategy._pixelShader = value;
                _mainContext.Strategy._pixelConstantBuffers.Clear();
                _mainContext.Strategy._pixelShaderDirty = true;
            }
        }

        internal void SetConstantBuffer(ShaderStage stage, int slot, ConstantBuffer buffer)
        {
            if (stage == ShaderStage.Vertex)
                _mainContext.Strategy._vertexConstantBuffers[slot] = buffer;
            else
                _mainContext.Strategy._pixelConstantBuffers[slot] = buffer;
        }

        public bool ResourcesLost { get; set; }

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        /// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
        [Obsolete("Use DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount) instead. In future versions this method can be removed.")]
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
        }

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            if (_mainContext.Strategy._vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");

            if (_mainContext.Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");

            if (_mainContext.Strategy._indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");

            if (this.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < GraphicsCapabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            PlatformDrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in an array of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in the given array of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (vertexData == null)
                throw new ArgumentNullException("vertexData");

            if (vertexData.Length == 0)
                throw new ArgumentOutOfRangeException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (this.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < GraphicsCapabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            var vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

            if (vertexOffset + vertexCount > vertexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            PlatformDrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, vertexDeclaration, vertexCount);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw.</param>
        /// <param name="vertexStart">Index of the vertex to start at.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            if (_mainContext.Strategy._vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawPrimitives.");

            if (_mainContext.Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawPrimitives.");

            if (this.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < GraphicsCapabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            var vertexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

            PlatformDrawPrimitives(primitiveType, vertexStart, vertexCount);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            // These parameter checks are a duplicate of the checks in the int[] overload of DrawUserIndexedPrimitives.
            // Inlined here for efficiency.

            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");

            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");

            if (this.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < GraphicsCapabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }


            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            PlatformDrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <remarks>The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.</remarks>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount) where T : struct, IVertexType
        {
            DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, VertexDeclarationCache<T>.VertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (this.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support 32 bit indices");

            // These parameter checks are a duplicate of the checks in the short[] overload of DrawUserIndexedPrimitives.
            // Inlined here for efficiency.

            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");

            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");

            if (this.GraphicsProfile == GraphicsProfile.Reach && primitiveCount > 65535)
                throw new NotSupportedException("Reach profile supports a maximum of 65535 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.HiDef && primitiveCount > 1048575)
                throw new NotSupportedException("HiDef profile supports a maximum of 1048575 primitives per draw call.");
            if (this.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < GraphicsCapabilities.MaxTextureSlots; i++)
                {
                    var tx2D = Textures[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        ||  SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }


            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            PlatformDrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += primitiveCount; }
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="minVertexIndex">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="numVertices">This is unused and remains here only for XNA API compatibility.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Note that minVertexIndex and numVertices are unused in MonoGame and will be ignored.</remarks>
        [Obsolete("Use DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount) instead. In future versions this method can be removed.")]
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, int instanceCount)
        {
            DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
            DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="baseInstance">Used to offset the instance range indexed from the instance buffer.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (this.GraphicsProfile == GraphicsProfile.Reach)
                throw new NotSupportedException("Reach profile does not support Instancing.");

            if (_mainContext.Strategy._vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");

            if (_mainContext.Strategy._vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");

            if (_mainContext.Strategy._indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            PlatformDrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);

            unchecked { CurrentContext._graphicsMetrics._drawCount++; }
            unchecked { CurrentContext._graphicsMetrics._primitiveCount += (primitiveCount * instanceCount); }
        }

        /// <summary>
        /// Gets the Pixel data of what is currently drawn on screen.
        /// The format is whatever the current format of the backbuffer is.
        /// </summary>
        /// <typeparam name="T">A byte[] of size (ViewPort.Width * ViewPort.Height * 4)</typeparam>
        public void GetBackBufferData<T>(T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            GetBackBufferData(null, data, 0, data.Length);
        }

        public void GetBackBufferData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            GetBackBufferData(null, data, startIndex, elementCount);
        }

        public void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            int width, height;
            if (rect.HasValue)
            {
                var rectangle = rect.Value;
                width = rectangle.Width;
                height = rectangle.Height;

                if (rectangle.X < 0 || rectangle.Y < 0 || rectangle.Width <= 0 || rectangle.Height <= 0 ||
                    rectangle.Right > PresentationParameters.BackBufferWidth || rectangle.Top > PresentationParameters.BackBufferHeight)
                    throw new ArgumentException("Rectangle must fit in BackBuffer dimensions");
            }
            else
            {
                width = PresentationParameters.BackBufferWidth;
                height = PresentationParameters.BackBufferHeight;
            }

            var tSize = ReflectionHelpers.SizeOf<T>();
            var fSize = PresentationParameters.BackBufferFormat.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");
            var dataByteSize = width * height * fSize;

            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1} bytes.",
                                            elementCount * tSize, dataByteSize), "elementCount");

            PlatformGetBackBufferData(rect, data, startIndex, elementCount);
        }

        // uniformly scales down the given rectangle by 10%
        internal static Rectangle GetDefaultTitleSafeArea(int x, int y, int width, int height)
        {
            var marginX = (width + 19) / 20;
            var marginY = (height + 19) / 20;
            x += marginX;
            y += marginY;

            width -= marginX * 2;
            height -= marginY * 2;
            return new Rectangle(x, y, width, height);
        }

        internal static Rectangle GetTitleSafeArea(int x, int y, int width, int height)
        {
            return PlatformGetTitleSafeArea(x, y, width, height);
        }
    }
}
