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
