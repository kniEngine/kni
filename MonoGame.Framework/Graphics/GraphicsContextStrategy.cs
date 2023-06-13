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
        protected GraphicsDevice Device { get; private set; }

        private bool _isDisposed = false;

        internal Rectangle _scissorRectangle;
        internal bool _scissorRectangleDirty;

        // states
        internal BlendState _blendState;
        internal Color _blendFactor = Color.White;
        internal DepthStencilState _depthStencilState;
        internal RasterizerState _rasterizerState;
        internal SamplerStateCollection _samplerStates;
        internal SamplerStateCollection _vertexSamplerStates;

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
        internal Shader _vertexShader;
        internal Shader _pixelShader;
        internal readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);
        internal readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);

        // shaders dirty flags
        internal bool _vertexShaderDirty;
        internal bool _pixelShaderDirty;

        // buffers
        internal IndexBuffer _indexBuffer;
        internal VertexBufferBindings _vertexBuffers;

        // buffers dirty flags
        internal bool _indexBufferDirty;
        internal bool _vertexBuffersDirty;

        // textures
        internal TextureCollection _textures;
        internal TextureCollection _vertexTextures;

        internal readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[8];
        internal int _currentRenderTargetCount;
        internal readonly RenderTargetBinding[] _singleRenderTargetBinding = new RenderTargetBinding[1];

        public int RenderTargetCount { get { return _currentRenderTargetCount; } }

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

                if (newBlendState.IndependentBlendEnable && !Device.GraphicsCapabilities.SupportsSeparateBlendStates)
                    throw new PlatformNotSupportedException("Independent blend states requires at least OpenGL 4.0 or GL_ARB_draw_buffers_blend. Try upgrading your graphics drivers.");

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(this.Device);

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

                newDepthStencilState.BindToGraphicsDevice(this.Device);

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

                if (!value.DepthClipEnable && !Device.GraphicsCapabilities.SupportsDepthClamp)
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

                newRasterizerState.BindToGraphicsDevice(this.Device);

                _actualRasterizerState = newRasterizerState;

                _rasterizerStateDirty = true;

            }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _samplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return _vertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return _textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return _vertexTextures; }
        }


        public IndexBuffer Indices
        {
            get { return _indexBuffer; }
            set
            {
                if (_indexBuffer == value)
                    return;

                _indexBuffer = value;
                _indexBufferDirty = true;
            }
        }


        protected GraphicsContextStrategy(GraphicsDevice device)
        {
            Device = device;
            
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

        #region IDisposable Members

        ~GraphicsContextStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                _isDisposed = true;
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
