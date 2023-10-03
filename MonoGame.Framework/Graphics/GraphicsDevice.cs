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
        private GraphicsDeviceStrategy _strategy;

        internal GraphicsDeviceStrategy Strategy { get { return _strategy; } }

        internal GraphicsContext CurrentContext { get { return _strategy.CurrentContext; } }


        // TODO: Graphics Device events need implementing
        public event EventHandler<EventArgs> DeviceLost;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
        public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;

        /// <summary>
        /// Raised when the GraphicsDevice is disposed or finalized.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        internal event EventHandler<PresentationEventArgs> PresentationChanged;

        public bool IsDisposed
        {
            get { return _strategy.IsDisposed; }
        }

        public GraphicsAdapter Adapter
        {
            get { return _strategy.Adapter; }
        }

        public GraphicsProfile GraphicsProfile
        {
            get { return _strategy.GraphicsProfile; }
        }

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
        public bool UseHalfPixelOffset
        {
            get { return _strategy.UseHalfPixelOffset; }
        }

        public PresentationParameters PresentationParameters
        {
            get { return _strategy.PresentationParameters; }
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

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug
        {
            get { return CurrentContext.GraphicsDebug ; }
            set { CurrentContext.GraphicsDebug = value; }
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
            _strategy = new ConcreteGraphicsDevice(this, adapter, graphicsProfile, false, presentationParameters);
            _strategy.DeviceResetting += (sender, e) => { OnDeviceResetting(e); };
            _strategy.DeviceReset += (sender, e) => { OnDeviceReset(e); };
            _strategy.PresentationChanged += (sender, e) => { OnPresentationChanged(e); };
            _strategy.Disposing += (sender, e) => { OnDisposing(e); };

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
            _strategy = new ConcreteGraphicsDevice(this, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters);
            _strategy.DeviceResetting += (sender, e) => { OnDeviceResetting(e); };
            _strategy.DeviceReset += (sender, e) => { OnDeviceReset(e); };
            _strategy.PresentationChanged += (sender, e) => { OnPresentationChanged(e); };
            _strategy.Disposing += (sender, e) => { OnDisposing(e); };

            Initialize();
        }

        ~GraphicsDevice()
        {
            Strategy.OnDisposing(EventArgs.Empty);
            Dispose(false);
        }


        private void Initialize()
        {
            // Setup
            _strategy.ToConcrete<ConcreteGraphicsDevice>().PlatformSetup();

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
            _strategy._mainContext.Strategy._viewport = new Viewport(0, 0, DisplayMode.Width, DisplayMode.Height);

            _strategy._mainContext.Strategy._vertexTextures = new TextureCollection(_strategy._mainContext.Strategy, Strategy.Capabilities.MaxVertexTextureSlots);
            _strategy._mainContext.Strategy._pixelTextures = new TextureCollection(_strategy._mainContext.Strategy, Strategy.Capabilities.MaxTextureSlots);

            _strategy._mainContext.Strategy._pixelSamplerStates = new SamplerStateCollection(_strategy._mainContext.Strategy, Strategy.Capabilities.MaxTextureSlots);
            _strategy._mainContext.Strategy._vertexSamplerStates = new SamplerStateCollection(_strategy._mainContext.Strategy, Strategy.Capabilities.MaxVertexTextureSlots);

            _strategy._mainContext.Strategy._blendStateAdditive = BlendState.Additive.Clone();
            _strategy._mainContext.Strategy._blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
            _strategy._mainContext.Strategy._blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
            _strategy._mainContext.Strategy._blendStateOpaque = BlendState.Opaque.Clone();

            _strategy._mainContext.Strategy.BlendState = BlendState.Opaque;

            _strategy._mainContext.Strategy._depthStencilStateDefault = DepthStencilState.Default.Clone();
            _strategy._mainContext.Strategy._depthStencilStateDepthRead = DepthStencilState.DepthRead.Clone();
            _strategy._mainContext.Strategy._depthStencilStateNone = DepthStencilState.None.Clone();

            _strategy._mainContext.Strategy.DepthStencilState = DepthStencilState.Default;

            _strategy._mainContext.Strategy._rasterizerStateCullClockwise = RasterizerState.CullClockwise.Clone();
            _strategy._mainContext.Strategy._rasterizerStateCullCounterClockwise = RasterizerState.CullCounterClockwise.Clone();
            _strategy._mainContext.Strategy._rasterizerStateCullNone = RasterizerState.CullNone.Clone();

            _strategy._mainContext.Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Setup end

            _strategy.ToConcrete<ConcreteGraphicsDevice>().PlatformInitialize();

            // Force set the default render states.
            _strategy._mainContext.Strategy._blendStateDirty = true;
            _strategy._mainContext.Strategy._blendFactorDirty = true;
            _strategy._mainContext.Strategy._depthStencilStateDirty = true;
            _strategy._mainContext.Strategy._rasterizerStateDirty = true;
            _strategy._mainContext.Strategy.BlendState = BlendState.Opaque;
            _strategy._mainContext.Strategy.DepthStencilState = DepthStencilState.Default;
            _strategy._mainContext.Strategy.RasterizerState = RasterizerState.CullCounterClockwise;

            // Force set the buffers and shaders on next ApplyState() call
            _strategy._mainContext.Strategy._vertexBuffers = new VertexBufferBindings(Strategy.Capabilities.MaxVertexBufferSlots);
            _strategy._mainContext.Strategy._vertexBuffersDirty = true;
            _strategy._mainContext.Strategy._indexBufferDirty = true;
            _strategy._mainContext.Strategy._vertexShaderDirty = true;
            _strategy._mainContext.Strategy._pixelShaderDirty = true;

            // Set the default scissor rect.
            _strategy._mainContext.Strategy._scissorRectangleDirty = true;
            _strategy._mainContext.Strategy._scissorRectangle = _strategy._mainContext.Strategy._viewport.Bounds;

            // Set the default render target.
            _strategy._mainContext.ApplyRenderTargets(null);
        }

        public Rectangle ScissorRectangle
        {
            get { return CurrentContext.ScissorRectangle; }
            set { CurrentContext.ScissorRectangle = value; }
        }

        public Viewport Viewport
        {
            get { return CurrentContext.Viewport; }
            set { CurrentContext.Viewport = value; }
        }

        public BlendState BlendState
        {
            get { return CurrentContext.BlendState; }
            set { CurrentContext.BlendState = value; }
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
            get { return CurrentContext.BlendFactor; }
            set { CurrentContext.BlendFactor = value; }
        }

        public DepthStencilState DepthStencilState
        {
            get { return CurrentContext.DepthStencilState; }
            set { CurrentContext.DepthStencilState = value; }
        }

        public RasterizerState RasterizerState
        {
            get { return CurrentContext.RasterizerState; }
            set { CurrentContext.RasterizerState = value; }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return CurrentContext.SamplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return CurrentContext.VertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return CurrentContext.Textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return CurrentContext.VertexTextures; }
        }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public Color DiscardColor
        {
            get { return CurrentContext.DiscardColor; }
            set { CurrentContext.DiscardColor = value; }
        }

        public void Clear(Color color)
        {
            CurrentContext.Clear(color);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            CurrentContext.Clear(options, color, depth, stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            CurrentContext.Clear(options, color, depth, stencil);
        }

        public void Dispose()
        {
            if (!_strategy.IsDisposed)
            {
                Strategy.OnDisposing(EventArgs.Empty);
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
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                _strategy.DeviceResetting -= (sender, e) => { OnDeviceResetting(e); };
                _strategy.DeviceReset -= (sender, e) => { OnDeviceReset(e); };
                _strategy.PresentationChanged -= (sender, e) => { OnPresentationChanged(e); };
                _strategy.Disposing -= (sender, e) => { OnDisposing(e); };

                _strategy.Dispose();
            }
        }


        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (_strategy._mainContext.IsRenderTargetBound)
                throw new InvalidOperationException("Cannot call Present when a render target is active.");

            Strategy.Present();
        }

        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            Strategy.Present(sourceRectangle, destinationRectangle, overrideWindowHandle);
        }

        private void OnDeviceResetting(EventArgs e)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, e);
        }

        private void OnPresentationChanged(PresentationEventArgs e)
        {
            var handler = PresentationChanged;
            if (handler != null)
                handler(this, e);
        }

        private void OnDeviceReset(EventArgs e)
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(this, e);
        }

        public void Reset()
        {
            Strategy.OnDeviceResetting(EventArgs.Empty);

            Strategy.Reset();

            // Update the back buffer.
            Strategy.ToConcrete<ConcreteGraphicsDevice>().OnPresentationChanged();
            
            Strategy.OnPresentationChanged(new PresentationEventArgs(PresentationParameters));

            Strategy.OnDeviceReset(EventArgs.Empty);
        }

        public void Reset(PresentationParameters presentationParameters)
        {
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            Strategy.OnDeviceResetting(EventArgs.Empty);

            Strategy.Reset(presentationParameters);

            Strategy.OnDeviceReset(EventArgs.Empty);
        }

        public DisplayMode DisplayMode
        {
            get { return Adapter.CurrentDisplayMode; }
        }

        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get { return GraphicsDeviceStatus.Normal; }
        }



        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            CurrentContext.SetRenderTarget(renderTarget);
        }

        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            CurrentContext.SetRenderTarget(renderTarget, cubeMapFace);
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            CurrentContext.SetRenderTarget(renderTarget, arraySlice);
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget3D renderTarget, int arraySlice)
        {
            CurrentContext.SetRenderTarget(renderTarget, arraySlice);
        }

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
            CurrentContext.SetRenderTargets(renderTargets);
        }

        public int RenderTargetCount
        {
            get { return CurrentContext.RenderTargetCount; }
        }

        public RenderTargetBinding[] GetRenderTargets()
        {
            RenderTargetBinding[] bindings = new RenderTargetBinding[RenderTargetCount];
            GetRenderTargets(bindings);
            return bindings;
        }

        public void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            CurrentContext.GetRenderTargets(bindings);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            CurrentContext.SetVertexBuffer(vertexBuffer);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            CurrentContext.SetVertexBuffer(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
        {
            CurrentContext.SetVertexBuffers(vertexBuffers);
        }

        public IndexBuffer Indices
        {
            get { return CurrentContext.Indices; }
            set { CurrentContext.Indices = value; }
        }

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
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            CurrentContext.DrawIndexedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount);
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
            CurrentContext.DrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);
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
            CurrentContext.DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount);
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
            CurrentContext.DrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, primitiveCount, vertexDeclaration);
        }

        /// <summary>
        /// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw.</param>
        /// <param name="vertexStart">Index of the vertex to start at.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            CurrentContext.DrawPrimitives(primitiveType, vertexStart, primitiveCount);
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
            CurrentContext.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount);
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
             CurrentContext.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
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
            CurrentContext.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount);
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
            CurrentContext.DrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);
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
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, int instanceCount)
        {
            CurrentContext.DrawInstancedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount, instanceCount);
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
            CurrentContext.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, instanceCount);
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
            CurrentContext.DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);
        }

        /// <summary>
        /// Sends queued-up commands in the command buffer to the graphics processing unit (GPU).
        /// </summary>
        public void Flush()
        {
            CurrentContext.Flush();
        }

        /// <summary>
        /// Gets the Pixel data of what is currently drawn on screen.
        /// The format is whatever the current format of the backbuffer is.
        /// </summary>
        /// <typeparam name="T">A byte[] of size (ViewPort.Width * ViewPort.Height * 4)</typeparam>
        public void GetBackBufferData<T>(T[] data) where T : struct
        {
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
                Rectangle rectangle = rect.Value;
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

            int tSize = ReflectionHelpers.SizeOf<T>();
            int fSize = PresentationParameters.BackBufferFormat.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");
            int dataByteSize = width * height * fSize;

            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1} bytes.",
                                            elementCount * tSize, dataByteSize), "elementCount");

            Strategy.GetBackBufferData(rect, data, startIndex, elementCount);
        }

        // uniformly scales down the given rectangle by 10%
        internal static Rectangle GetDefaultTitleSafeArea(int x, int y, int width, int height)
        {
            int marginX = (width + 19) / 20;
            int marginY = (height + 19) / 20;
            x += marginX;
            y += marginY;

            width -= marginX * 2;
            height -= marginY * 2;
            return new Rectangle(x, y, width, height);
        }

        internal static Rectangle GetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
    }
}
