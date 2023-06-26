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

        public Viewport Viewport
        {
            get { return Strategy.Viewport; }
            set { Strategy.Viewport = value; }
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
        internal bool IsRenderTargetBound { get { return Strategy.IsRenderTargetBound; } }

        public void Clear(Color color)
        {
            ClearOptions options = ClearOptions.Target
                                 | ClearOptions.DepthBuffer
                                 | ClearOptions.Stencil;
            Strategy.Clear(options, color.ToVector4(), Strategy.Viewport.MaxDepth, 0);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Strategy.Clear(options, color.ToVector4(), depth, stencil);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            Strategy.Clear(options, color, depth, stencil);

            unchecked { _graphicsMetrics._clearCount++; }
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            Strategy.SetVertexBuffer(vertexBuffer);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            Strategy.SetVertexBuffer(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(VertexBufferBinding[] vertexBuffers)
        {
            Strategy.SetVertexBuffers(vertexBuffers);
        }

        internal Shader VertexShader
        {
            get { return Strategy.VertexShader; }
            set { Strategy.VertexShader = value; }
        }

        internal Shader PixelShader
        {
            get { return Strategy.PixelShader; }
            set { Strategy.PixelShader = value; }
        }

        public void GetRenderTargets(RenderTargetBinding[] bindings)
        {
            Strategy.GetRenderTargets(bindings);
        }

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
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
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
            }
        }

        /// <remarks>Only implemented for DirectX </remarks>
        public void SetRenderTarget(RenderTarget2D renderTarget, int arraySlice)
        {
            if (!_device.Capabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
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
                Strategy._singleRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(Strategy._singleRenderTargetBinding);
            }
        }

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
            // Avoid having to check for both null and zero length array.
            int renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                    renderTargets = null;
            }

            if (_device.GraphicsProfile == GraphicsProfile.Reach && renderTargetCount > 1)
                throw new NotSupportedException("Reach profile supports a maximum of 1 simultaneous rendertargets");
            if (_device.GraphicsProfile == GraphicsProfile.HiDef && renderTargetCount > 4)
                throw new NotSupportedException("HiDef profile supports a maximum of 4 simultaneous rendertargets");
            if (renderTargetCount > 8)
                throw new NotSupportedException("Current profile supports a maximum of 8 simultaneous rendertargets");

            // Try to early out if the current and new bindings are equal.
            if (Strategy._currentRenderTargetCount == renderTargetCount)
            {
                bool isEqual = true;
                for (int i = 0; i < Strategy._currentRenderTargetCount; i++)
                {
                    if (Strategy._currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        Strategy._currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
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
                unchecked { _graphicsMetrics._targetCount++; }
            else
                unchecked { _graphicsMetrics._targetCount += renderTargetCount; }
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            bool clearTarget = false;

            ((ConcreteGraphicsContext)Strategy).PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(Strategy._currentRenderTargetBindings, 0, Strategy._currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                Strategy._currentRenderTargetCount = 0;

                ((ConcreteGraphicsContext)Strategy).PlatformApplyDefaultRenderTarget();
                clearTarget = _device.PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = _device.PresentationParameters.BackBufferWidth;
                renderTargetHeight = _device.PresentationParameters.BackBufferHeight;
            }
            else
            {
                // Copy the new bindings.
                Array.Copy(renderTargets, Strategy._currentRenderTargetBindings, renderTargets.Length);
                Strategy._currentRenderTargetCount = renderTargets.Length;

                IRenderTarget renderTarget = ((ConcreteGraphicsContext)Strategy).PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Strategy.Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            Strategy.ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            if (clearTarget)
                Clear(_device.DiscardColor);
        }


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
