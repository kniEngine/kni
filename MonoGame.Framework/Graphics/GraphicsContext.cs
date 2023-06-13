// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    internal sealed class GraphicsContext : IDisposable
    {
        private GraphicsDevice _device;
        private GraphicsContextStrategy _strategy;
        private bool _isDisposed = false;


        internal GraphicsMetrics _graphicsMetrics;

        internal GraphicsContextStrategy Strategy { get { return _strategy; } }

        internal GraphicsContext(GraphicsDevice device, GraphicsContextStrategy strategy)
        {
            _device = device;
            _strategy = strategy;

        }


        /// <summary>
        /// The rendering information for debugging and profiling.
        /// The metrics are reset every frame after draw within <see cref="GraphicsDevice.Present"/>. 
        /// </summary>
        public GraphicsMetrics Metrics
        {
            get { return _graphicsMetrics; }
            set { _graphicsMetrics = value; }
        }

        public Rectangle ScissorRectangle
        {
            get { return Strategy.ScissorRectangle; }
            set { Strategy.ScissorRectangle = value; }
        }

        public BlendState BlendState
        {
            get { return Strategy.BlendState; }
            set { Strategy.BlendState = value; }
        }

        public Color BlendFactor
        {
            get { return Strategy.BlendFactor; }
            set { Strategy.BlendFactor = value; }
        }

        public DepthStencilState DepthStencilState
        {
            get { return Strategy.DepthStencilState; }
            set { Strategy.DepthStencilState = value; }
        }

        public RasterizerState RasterizerState
        {
            get { return Strategy.RasterizerState; }
            set { Strategy.RasterizerState = value; }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return Strategy.SamplerStates; }
        }

        public SamplerStateCollection VertexSamplerStates
        {
            get { return Strategy.VertexSamplerStates; }
        }

        public TextureCollection Textures
        {
            get { return Strategy.Textures; }
        }

        public TextureCollection VertexTextures
        {
            get { return Strategy.VertexTextures; }
        }

        public IndexBuffer Indices
        {
            get { return Strategy.Indices; }
            set { Strategy.Indices = value; }
        }


        public int RenderTargetCount { get { return Strategy.RenderTargetCount; } }

        #region IDisposable Members

        ~GraphicsContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                _strategy.Dispose();

                _strategy = null;
                _device = null;
                _isDisposed = true;
            }
        }

        //[Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (!_isDisposed)
                return;

            throw new ObjectDisposedException("Object is Disposed.");
        }

        #endregion IDisposable Members

    }
}
