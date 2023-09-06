// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsContextStrategy : IDisposable
    {
        private GraphicsContext _context;

        internal GraphicsContext Context { get { return _context; } }

        private bool _isDisposed = false;

        internal Rectangle _scissorRectangle;
        internal bool _scissorRectangleDirty;
        internal Viewport _viewport;

        // states
        private BlendState _blendState;
        private Color _blendFactor = Color.White;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        internal SamplerStateCollection _vertexSamplerStates;
        internal SamplerStateCollection _pixelSamplerStates;

        // states dirty flags
        internal bool _blendStateDirty;
        internal bool _blendFactorDirty;
        internal bool _depthStencilStateDirty;
        internal bool _rasterizerStateDirty;

        // actual states
        internal BlendState _actualBlendState;
        internal DepthStencilState _actualDepthStencilState;
        internal RasterizerState _actualRasterizerState;

        // predefined states
        internal BlendState _blendStateAdditive;
        internal BlendState _blendStateAlphaBlend;
        internal BlendState _blendStateNonPremultiplied;
        internal BlendState _blendStateOpaque;
        internal DepthStencilState _depthStencilStateDefault;
        internal DepthStencilState _depthStencilStateDepthRead;
        internal DepthStencilState _depthStencilStateNone;
        internal RasterizerState _rasterizerStateCullClockwise;
        internal RasterizerState _rasterizerStateCullCounterClockwise;
        internal RasterizerState _rasterizerStateCullNone;

        // shaders
        private Shader _vertexShader;
        private Shader _pixelShader;
        internal readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);
        internal readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);

        // shaders dirty flags
        internal bool _vertexShaderDirty;
        internal bool _pixelShaderDirty;

        // buffers
        private IndexBuffer _indexBuffer;
        internal VertexBufferBindings _vertexBuffers;

        // buffers dirty flags
        internal bool _indexBufferDirty;
        internal bool _vertexBuffersDirty;

        // textures
        internal TextureCollection _vertexTextures;
        internal TextureCollection _pixelTextures;

        internal readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[8];
        internal int _currentRenderTargetCount;
        internal readonly RenderTargetBinding[] _singleRenderTargetBinding = new RenderTargetBinding[1];

        private Color _discardColor = new Color(68, 34, 136, 255);

        private GraphicsDebug _graphicsDebug;


        public int RenderTargetCount { get { return _currentRenderTargetCount; } }
        internal bool IsRenderTargetBound { get { return _currentRenderTargetCount > 0; } }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public Color DiscardColor
        {
            get { return _discardColor; }
            set { _discardColor = value; }
        }

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug
        {
            get { return _graphicsDebug; }
            set { _graphicsDebug = value; }
        }

        public Rectangle ScissorRectangle
        {
            get { return _scissorRectangle; }
            set
            {
                if (_scissorRectangle == value)
                    return;

                _scissorRectangle = value;
                _scissorRectangleDirty = true;
            }
        }

        virtual public Viewport Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_blendState == value)
                    return;

                _blendState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newBlendState = _blendState;
                if (ReferenceEquals(_blendState, BlendState.Additive))
                    newBlendState = _blendStateAdditive;
                else if (ReferenceEquals(_blendState, BlendState.AlphaBlend))
                    newBlendState = _blendStateAlphaBlend;
                else if (ReferenceEquals(_blendState, BlendState.NonPremultiplied))
                    newBlendState = _blendStateNonPremultiplied;
                else if (ReferenceEquals(_blendState, BlendState.Opaque))
                    newBlendState = _blendStateOpaque;

                if (newBlendState.IndependentBlendEnable && !Context.DeviceStrategy.Capabilities.SupportsSeparateBlendStates)
                    throw new PlatformNotSupportedException("Independent blend states requires at least OpenGL 4.0 or GL_ARB_draw_buffers_blend. Try upgrading your graphics drivers.");

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(this.Context.DeviceStrategy.Device);

                _actualBlendState = newBlendState;

                BlendFactor = _actualBlendState.BlendFactor;

                _blendStateDirty = true;
            }
        }

        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (_blendFactor == value)
                    return;
                _blendFactor = value;
                _blendFactorDirty = true;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_depthStencilState == value)
                    return;

                _depthStencilState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newDepthStencilState = _depthStencilState;
                if (ReferenceEquals(_depthStencilState, DepthStencilState.Default))
                    newDepthStencilState = _depthStencilStateDefault;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.DepthRead))
                    newDepthStencilState = _depthStencilStateDepthRead;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.None))
                    newDepthStencilState = _depthStencilStateNone;

                newDepthStencilState.BindToGraphicsDevice(this.Context.DeviceStrategy.Device);

                _actualDepthStencilState = newDepthStencilState;

                _depthStencilStateDirty = true;
            }
        }

        public RasterizerState RasterizerState
        {
            get { return _rasterizerState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_rasterizerState == value)
                    return;

                if (!value.DepthClipEnable && !Context.DeviceStrategy.Capabilities.SupportsDepthClamp)
                    throw new InvalidOperationException("Cannot set RasterizerState.DepthClipEnable to false on this graphics device");

                _rasterizerState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newRasterizerState = _rasterizerState;
                if (ReferenceEquals(_rasterizerState, RasterizerState.CullClockwise))
                    newRasterizerState = _rasterizerStateCullClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullCounterClockwise))
                    newRasterizerState = _rasterizerStateCullCounterClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullNone))
                    newRasterizerState = _rasterizerStateCullNone;

                newRasterizerState.BindToGraphicsDevice(this.Context.DeviceStrategy.Device);

                _actualRasterizerState = newRasterizerState;

                _rasterizerStateDirty = true;
            }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return _vertexSamplerStates; }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _pixelSamplerStates; }
        }

        public TextureCollection VertexTextures
        {
            get { return _vertexTextures; }
        }

        public TextureCollection Textures
        {
            get { return _pixelTextures; }
        }


        public IndexBuffer Indices
        {
            get { return _indexBuffer; }
            set
            {
                if (_indexBuffer != value)
                {
                    _indexBuffer = value;
                    _indexBufferDirty = true;
                }
            }
        }

        internal Shader VertexShader
        {
            get { return _vertexShader; }
            set
            {
                if (_vertexShader == value)
                    return;

                _vertexShader = value;
                _vertexConstantBuffers.Clear();
                _vertexShaderDirty = true;
            }
        }

        internal Shader PixelShader
        {
            get { return _pixelShader; }
            set
            {
                if (_pixelShader == value)
                    return;

                _pixelShader = value;
                _pixelConstantBuffers.Clear();
                _pixelShaderDirty = true;
            }
        }

        protected GraphicsContextStrategy(GraphicsContext context)
        {
            _context = context;
            
        }

        internal void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            if (vertexBuffer != null)
            {
                _vertexBuffersDirty |= _vertexBuffers.Set(vertexBuffer, 0);
            }
            else
            {
                _vertexBuffersDirty |= _vertexBuffers.Clear();
            }
        }

        public abstract void Clear(ClearOptions options, Vector4 color, float depth, int stencil);

        internal void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            if (vertexBuffer != null)
            {
                if (0 >= vertexOffset && vertexOffset < vertexBuffer.VertexCount)
                {
                    _vertexBuffersDirty |= _vertexBuffers.Set(vertexBuffer, vertexOffset);
                }
                else
                    throw new ArgumentOutOfRangeException("vertexOffset");
            }
            else
            {
                if (vertexOffset == 0)
                {
                    _vertexBuffersDirty |= _vertexBuffers.Clear();
                }
                else
                    throw new ArgumentOutOfRangeException("vertexOffset");
            }
        }

        internal void SetVertexBuffers(VertexBufferBinding[] vertexBuffers)
        {
            if (vertexBuffers != null && vertexBuffers.Length > 0)
            {
                if (vertexBuffers.Length <= this.Context.DeviceStrategy.Capabilities.MaxVertexBufferSlots)
                {
                    _vertexBuffersDirty |= _vertexBuffers.Set(vertexBuffers);
                }
                else
                {
                    var message = string.Format("Max number of vertex buffers is {0}.", this.Context.DeviceStrategy.Capabilities.MaxVertexBufferSlots);
                    throw new ArgumentOutOfRangeException("vertexBuffers", message);
                }
            }
            else
            {
                _vertexBuffersDirty |= _vertexBuffers.Clear();
            }
        }

        internal void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            Debug.Assert(bindings.Length == _currentRenderTargetCount, "Invalid bindings array length!");

            Array.Copy(_currentRenderTargetBindings, bindings, _currentRenderTargetCount);
        }

        internal static int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.TriangleList:
                    return primitiveCount * 3;
                case PrimitiveType.TriangleStrip:
                    return primitiveCount + 2;
                case PrimitiveType.PointList:
                    return primitiveCount;
                default:
                    throw new NotSupportedException();
            }
        }

        public abstract void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount);
        public abstract void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount);
        public abstract void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount);
        public abstract void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct;
        public abstract void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct;
        public abstract void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct;
        public abstract void Flush();


        internal abstract GraphicsDebugStrategy CreateGraphicsDebugStrategy();
        internal abstract TextureCollectionStrategy CreateTextureCollectionStrategy(int capacity);
        internal abstract SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(int capacity);

        internal abstract ITexture2DStrategy CreateTexture2DStrategy(int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared);
        internal abstract ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format);
        internal abstract ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format);
        internal abstract IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat);
        internal abstract IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat);
        internal abstract IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat);

        internal T ToConcrete<T>() where T : GraphicsContextStrategy
        {
            return (T)this;
        }


        #region IDisposable Members

        ~GraphicsContextStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blendState = null;
                _actualBlendState = null;
                _blendStateAdditive.Dispose();
                _blendStateAlphaBlend.Dispose();
                _blendStateNonPremultiplied.Dispose();
                _blendStateOpaque.Dispose();

                _depthStencilState = null;
                _actualDepthStencilState = null;
                _depthStencilStateDefault.Dispose();
                _depthStencilStateDepthRead.Dispose();
                _depthStencilStateNone.Dispose();

                _rasterizerState = null;
                _actualRasterizerState = null;
                _rasterizerStateCullClockwise.Dispose();
                _rasterizerStateCullCounterClockwise.Dispose();
                _rasterizerStateCullNone.Dispose();
            }
        }

        //[Conditional("DEBUG")]
        protected void ThrowIfDisposed()
        {
            if (!_isDisposed)
                return;

            throw new ObjectDisposedException("Object is Disposed.");
        }

        #endregion IDisposable Members

    }
}
