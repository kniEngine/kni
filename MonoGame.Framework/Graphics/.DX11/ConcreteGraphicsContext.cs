// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using SharpDX.Mathematics.Interop;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {
        private D3D11.DeviceContext _d3dContext;
        internal int _vertexBufferSlotsUsed;

        private PrimitiveType _lastPrimitiveType = (PrimitiveType)(-1);

        // The active render targets.
        internal readonly D3D11.RenderTargetView[] _currentRenderTargets = new D3D11.RenderTargetView[8];

        // The active depth view.
        internal D3D11.DepthStencilView _currentDepthStencilView;

        internal readonly Dictionary<VertexDeclaration, DynamicVertexBuffer> _userVertexBuffers = new Dictionary<VertexDeclaration, DynamicVertexBuffer>();
        internal DynamicIndexBuffer _userIndexBuffer16;
        internal DynamicIndexBuffer _userIndexBuffer32;


        internal D3D11.DeviceContext D3dContext { get { return _d3dContext; } }

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContext(GraphicsContext context, D3D11.DeviceContext d3dContext)
            : base(context)
        {
            _d3dContext = d3dContext;

#if WINDOWS
            GraphicsDebug = new GraphicsDebug(this);
#endif

        }

        public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // Clear options for depth/stencil buffer if not attached.
            if (_currentDepthStencilView != null)
            {
                if (_currentDepthStencilView.Description.Format != DXGI.Format.D24_UNorm_S8_UInt)
                    options &= ~ClearOptions.Stencil;
            }
            else
            {
                options &= ~ClearOptions.DepthBuffer;
                options &= ~ClearOptions.Stencil;
            }

            lock (this.D3dContext)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) == ClearOptions.Target)
                {
                    foreach (D3D11.RenderTargetView view in _currentRenderTargets)
                    {
                        if (view != null)
                            this.D3dContext.ClearRenderTargetView(view, new RawColor4(color.X, color.Y, color.Z, color.W));
                    }
                }

                // Clear the depth/stencil render buffer.
                D3D11.DepthStencilClearFlags flags = 0;
                if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
                    flags |= D3D11.DepthStencilClearFlags.Depth;
                if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
                    flags |= D3D11.DepthStencilClearFlags.Stencil;

                if (flags != 0)
                    this.D3dContext.ClearDepthStencilView(_currentDepthStencilView, flags, depth, (byte)stencil);
            }
        }

        private void PlatformApplyState()
        {
            Debug.Assert(this.D3dContext != null, "The d3d context is null!");

            {
                PlatformApplyBlend();
            }

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            if (_scissorRectangleDirty)
            {
                PlatformApplyScissorRectangle();
                _scissorRectangleDirty = false;
            }
        }

        private void PlatformApplyBlend()
        {
            if (_blendStateDirty || _blendFactorDirty)
            {
                D3D11.BlendState blendState = _actualBlendState.GetDxState(this);
                RawColor4 blendFactor = ConcreteGraphicsContext.ToDXColor(BlendFactor);
                this.D3dContext.OutputMerger.SetBlendState(blendState, blendFactor);

                _blendStateDirty = false;
                _blendFactorDirty = false;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            this.D3dContext.Rasterizer.SetScissorRectangle(
                _scissorRectangle.X,
                _scissorRectangle.Y,
                _scissorRectangle.Right,
                _scissorRectangle.Bottom);
            _scissorRectangleDirty = false;
        }

        private static SharpDX.Mathematics.Interop.RawColor4 ToDXColor(Color blendFactor)
        {
            return new SharpDX.Mathematics.Interop.RawColor4(
                    blendFactor.R / 255.0f,
                    blendFactor.G / 255.0f,
                    blendFactor.B / 255.0f,
                    blendFactor.A / 255.0f);
        }

        internal void PlatformApplyViewport()
        {
            lock (this.D3dContext)
            {
                if (this.D3dContext != null)
                {
                    RawViewportF viewport = new RawViewportF
                    {
                        X = _viewport.X,
                        Y = _viewport.Y,
                        Width = _viewport.Width,
                        Height = _viewport.Height,
                        MinDepth = _viewport.MinDepth,
                        MaxDepth = _viewport.MaxDepth
                    };
                    this.D3dContext.Rasterizer.SetViewport(viewport);
                }
            }
        }

        private void PlatformApplyIndexBuffer()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_indexBufferDirty)
            {
                this.D3dContext.InputAssembler.SetIndexBuffer(
                    Indices.Buffer,
                    Indices.IndexElementSize == IndexElementSize.SixteenBits ?
                        DXGI.Format.R16_UInt : DXGI.Format.R32_UInt,
                    0);
                _indexBufferDirty = false;
            }
        }

        private void PlatformApplyVertexBuffers()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_vertexBuffersDirty)
            {
                for (int slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);
                    VertexBuffer vertexBuffer = vertexBufferBinding.VertexBuffer;
                    VertexDeclaration vertexDeclaration = vertexBuffer.VertexDeclaration;
                    int vertexStride = vertexDeclaration.VertexStride;
                    int vertexOffsetInBytes = vertexBufferBinding.VertexOffset * vertexStride;
                    this.D3dContext.InputAssembler.SetVertexBuffers(
                        slot, new D3D11.VertexBufferBinding(vertexBuffer.Buffer, vertexStride, vertexOffsetInBytes));
                }

                // TODO: do we need to reset the previously set slots?
                //for (int slot = _vertexBuffers.Count; slot < _vertexBufferSlotsUsed; slot++)
                //{
                //    this.D3dContext.InputAssembler.SetVertexBuffers(slot, new D3D11.VertexBufferBinding());
                //}

                _vertexBufferSlotsUsed = _vertexBuffers.Count;
            }
        }

        private void PlatformApplyShaders()
        {
            // NOTE: This code assumes CurrentD3DContext has been locked by the caller.

            if (_vertexShaderDirty)
            {
                this.D3dContext.VertexShader.Set(VertexShader.VertexShader);

                unchecked { this.Context._graphicsMetrics._vertexShaderCount++; }
            }
            if (_vertexShaderDirty || _vertexBuffersDirty)
            {
                this.D3dContext.InputAssembler.InputLayout = VertexShader.InputLayouts.GetOrCreate(_vertexBuffers);
                _vertexShaderDirty = false;
                _vertexBuffersDirty = false;
            }

            if (_pixelShaderDirty)
            {
                this.D3dContext.PixelShader.Set(PixelShader.PixelShader);
                _pixelShaderDirty = false;

                unchecked { this.Context._graphicsMetrics._pixelShaderCount++; }
            }
        }

        private void PlatformApplyShaderBuffers()
        {
            _vertexConstantBuffers.Apply(this);
            _pixelConstantBuffers.Apply(this);

            this.VertexTextures.Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply(this.D3dContext.VertexShader);
            this.VertexSamplerStates.Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply(this.D3dContext.VertexShader);
            this.Textures.Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply(this.D3dContext.PixelShader);
            this.SamplerStates.Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply(this.D3dContext.PixelShader);
        }

        private void PlatformApplyPrimitiveType(PrimitiveType primitiveType)
        {
            if (_lastPrimitiveType == primitiveType)
                return;

            this.D3dContext.InputAssembler.PrimitiveTopology = ConcreteGraphicsContext.ToPrimitiveTopology(primitiveType);
            _lastPrimitiveType = primitiveType;
        }

        private static D3D.PrimitiveTopology ToPrimitiveTopology(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return D3D.PrimitiveTopology.LineList;
                case PrimitiveType.LineStrip:
                    return D3D.PrimitiveTopology.LineStrip;
                case PrimitiveType.TriangleList:
                    return D3D.PrimitiveTopology.TriangleList;
                case PrimitiveType.TriangleStrip:
                    return D3D.PrimitiveTopology.TriangleStrip;
                case PrimitiveType.PointList:
                    return D3D.PrimitiveTopology.PointList;

                default:
                    throw new ArgumentException();
            }
        }

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            lock (this.D3dContext)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.Draw(vertexCount, vertexStart);
            }
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            lock (this.D3dContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
                this.D3dContext.DrawIndexed(indexCount, startIndex, baseVertex);
            }
        }

        public override void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex,
            int primitiveCount, int baseInstance, int instanceCount)
        {
            lock (this.D3dContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

                if (baseInstance > 0)
                {
                    if (!this.Context.DeviceStrategy.Capabilities.SupportsBaseIndexInstancing)
                        throw new PlatformNotSupportedException("Instanced geometry drawing with base instance not supported.");

                    this.D3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, baseInstance);
                }
                else
                {
                    this.D3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, 0);
                }
            }
        }

        private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDecl)
            where T : struct
        {
            DynamicVertexBuffer buffer;

            if (!_userVertexBuffers.TryGetValue(vertexDecl, out buffer) || buffer.VertexCount < vertexCount)
            {
                // Dispose the previous buffer if we have one.
                if (buffer != null)
                    buffer.Dispose();

                int requiredVertexCount = Math.Max(vertexCount, 4 * 256);
                requiredVertexCount = (requiredVertexCount + 255) & (~255); // grow in chunks of 256.
                buffer = new DynamicVertexBuffer(this.Context.DeviceStrategy.Device, vertexDecl, requiredVertexCount, BufferUsage.WriteOnly);
                _userVertexBuffers[vertexDecl] = buffer;
            }

            int startVertex = buffer.UserOffset;


            if ((vertexCount + buffer.UserOffset) < buffer.VertexCount)
            {
                buffer.UserOffset += vertexCount;
                buffer.SetData(startVertex * vertexDecl.VertexStride, vertexData, vertexOffset, vertexCount, vertexDecl.VertexStride, SetDataOptions.NoOverwrite);
            }
            else
            {
                buffer.UserOffset = vertexCount;
                buffer.SetData(vertexData, vertexOffset, vertexCount, SetDataOptions.Discard);
                startVertex = 0;
            }

            SetVertexBuffer(buffer);

            return startVertex;
        }

        private int SetUserIndexBuffer<T>(T[] indexData, int indexOffset, int indexCount)
            where T : struct
        {
            DynamicIndexBuffer buffer;

            int indexSize = ReflectionHelpers.SizeOf<T>();
            IndexElementSize indexElementSize = indexSize == 2 ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

            int requiredIndexCount = Math.Max(indexCount, 6 * 512);
            requiredIndexCount = (requiredIndexCount + 511) & (~511); // grow in chunks of 512.
            if (indexElementSize == IndexElementSize.SixteenBits)
            {
                if (_userIndexBuffer16 == null || _userIndexBuffer16.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer16 != null)
                        _userIndexBuffer16.Dispose();

                    _userIndexBuffer16 = new DynamicIndexBuffer(this.Context.DeviceStrategy.Device, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer16;
            }
            else
            {
                if (_userIndexBuffer32 == null || _userIndexBuffer32.IndexCount < requiredIndexCount)
                {
                    if (_userIndexBuffer32 != null)
                        _userIndexBuffer32.Dispose();

                    _userIndexBuffer32 = new DynamicIndexBuffer(this.Context.DeviceStrategy.Device, indexElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                }

                buffer = _userIndexBuffer32;
            }

            int startIndex = buffer.UserOffset;

            if ((indexCount + buffer.UserOffset) < buffer.IndexCount)
            {
                buffer.UserOffset += indexCount;
                buffer.SetData(startIndex * indexSize, indexData, indexOffset, indexCount, SetDataOptions.NoOverwrite);
            }
            else
            {
                startIndex = 0;
                buffer.UserOffset = indexCount;
                buffer.SetData(indexData, indexOffset, indexCount, SetDataOptions.Discard);
            }

            Indices = buffer;

            return startIndex;
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers.
            //       Bind directly to d3dContext and set dirty flags.
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (this.D3dContext)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.Draw(vertexCount, startVertex);
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            int startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (this.D3dContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            int startIndex = SetUserIndexBuffer(indexData, indexOffset, indexCount);

            lock (this.D3dContext)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();
                PlatformApplyShaderBuffers();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.DrawIndexed(indexCount, startIndex, startVertex);
            }
        }

        public override void Flush()
        {            
            this.D3dContext.Flush();
        }


        internal override GraphicsDebugStrategy CreateGraphicsDebugStrategy()
        {
            return new ConcreteGraphicsDebug(this);
        }

        internal override TextureCollectionStrategy CreateTextureCollectionStrategy(int capacity)
        {
            return new ConcreteTextureCollection(this, capacity);
        }

        internal override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(int capacity)
        {
            return new ConcreteSamplerStateCollection(this, capacity);
        }

        internal override ITexture2DStrategy CreateTexture2DStrategy(int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
        {
            return new ConcreteTexture2D(this, width, height, mipMap, format, arraySize, shared);
        }

        internal override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTexture3D(this, width, height, depth, mipMap, format);
        }

        internal override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTextureCube(this, size, mipMap, format);
        }

        internal override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
        {
            return new ConcreteRenderTarget2D(this, width, height, mipMap, arraySize, usage, preferredSurfaceFormat, preferredDepthFormat);
        }

        internal override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
        {
            return new ConcreteRenderTarget3D(this, width, height, depth, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat);
        }

        internal override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
        {
            return new ConcreteRenderTargetCube(this, size, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userIndexBuffer16 != null)
                    _userIndexBuffer16.Dispose();
                if (_userIndexBuffer32 != null)
                    _userIndexBuffer32.Dispose();

                foreach (DynamicVertexBuffer vb in _userVertexBuffers.Values)
                    vb.Dispose();

                if (_d3dContext != null)
                    _d3dContext.Dispose();
                _d3dContext = null;
            }

            base.Dispose(disposing);
        }

        internal void PlatformResolveRenderTargets()
        {
            for (int i = 0; i < _currentRenderTargetCount; i++)
            {
                RenderTargetBinding renderTargetBinding = _currentRenderTargetBindings[i];

                // Resolve MSAA render targets
                RenderTarget2D renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget != null && renderTarget.MultiSampleCount > 1)
                    renderTarget.ResolveSubresource();

                // Generate mipmaps.
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    lock (this.D3dContext)
                    {
                        this.D3dContext.GenerateMips(renderTargetBinding.RenderTarget.GetTextureStrategy<ConcreteTexture>().GetShaderResourceView());
                    }
                }
            }
        }

        internal void PlatformApplyDefaultRenderTarget()
        {
            // Set the default swap chain.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentRenderTargets[0] = this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._renderTargetView;
            _currentDepthStencilView = this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._depthStencilView;

            lock (this.D3dContext)
            {
                this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
            }
        }

        internal IRenderTarget PlatformApplyRenderTargets()
        {
            // Clear the current render targets.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentDepthStencilView = null;

            // Make sure none of the new targets are bound
            // to the device as a texture resource.
            lock (this.D3dContext)
            {
                this.VertexTextures.Strategy.ToConcrete<ConcreteTextureCollection>().ClearTargets(_currentRenderTargetBindings, this.D3dContext.VertexShader);
                this.Textures.Strategy.ToConcrete<ConcreteTextureCollection>().ClearTargets(_currentRenderTargetBindings, this.D3dContext.PixelShader);
            }

            for (int i = 0; i < _currentRenderTargetCount; i++)
            {
                RenderTargetBinding binding = _currentRenderTargetBindings[i];
                IRenderTargetDX11 targetDX = (IRenderTargetDX11)binding.RenderTarget;
                _currentRenderTargets[i] = targetDX.GetRenderTargetView(binding.ArraySlice);
            }

            // Use the depth from the first target.
            IRenderTargetDX11 renderTargetDX = (IRenderTargetDX11)_currentRenderTargetBindings[0].RenderTarget;
            _currentDepthStencilView = renderTargetDX.GetDepthStencilView(_currentRenderTargetBindings[0].ArraySlice);

            // Set the targets.
            lock (this.D3dContext)
            {
                this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
            }

            return (IRenderTarget)_currentRenderTargetBindings[0].RenderTarget;
        }


#if WINDOWS_UAP
        internal void UAP_ResetRenderTargets()
        {
            PlatformApplyViewport();
                        
            lock (this.D3dContext)
                    this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);

            _pixelTextures.Dirty();
            this.SamplerStates.Dirty();
            _depthStencilStateDirty = true;
            _blendStateDirty = true;
            _indexBufferDirty = true;
            _vertexBuffersDirty = true;
            _pixelShaderDirty = true;
            _vertexShaderDirty = true;
            _rasterizerStateDirty = true;
            _scissorRectangleDirty = true;
        }
#endif

    }
}
