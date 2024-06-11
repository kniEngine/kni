// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private readonly Dictionary<VertexDeclaration, UserVertexBuffer> _userVertexBuffers = new Dictionary<VertexDeclaration, UserVertexBuffer>();
        private readonly UserIndexBuffer _userIndexBuffer16 = new UserIndexBuffer(IndexElementSize.SixteenBits);
        private readonly UserIndexBuffer _userIndexBuffer32 = new UserIndexBuffer(IndexElementSize.ThirtyTwoBits);


        private class UserVertexBuffer
        {
            public int Count;
            public int Offset;
            public DynamicVertexBuffer Buffer;
            public int Stride;

            public UserVertexBuffer(int vertexStride)
            {
                this.Count = -1;
                this.Offset = 0;
                this.Buffer = null;
                this.Stride = vertexStride;
            }
        }

        private class UserIndexBuffer
        {
            public int Count;
            public int Offset;
            public DynamicIndexBuffer Buffer;
            public readonly IndexElementSize ElementSize;

            public UserIndexBuffer(IndexElementSize indexElementSize)
            {
                this.Count =-1;
                this.Offset = 0;
                this.Buffer = null;
                this.ElementSize = indexElementSize;
            }
        }


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

#if WINDOWSDX
            GraphicsDebug = base.CreateGraphicsDebug();
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

            lock (this.SyncHandle)
            {
                // Clear the diffuse render buffer.
                if ((options & ClearOptions.Target) == ClearOptions.Target)
                {
                    foreach (D3D11.RenderTargetView view in _currentRenderTargets)
                    {
                        if (view != null)
                            this.D3dContext.ClearRenderTargetView(view, color.ToRawColor4());
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

                base.Metrics_AddClearCount();
            }
        }

        private void PlatformApplyState()
        {
            Debug.Assert(this.D3dContext != null, "The d3d context is null!");

            // NOTE: We make the assumption here that the caller has locked the d3dContext for us to use.

            {
                PlatformApplyBlend();
            }

            if (_depthStencilStateDirty)
            {
                D3D11.DepthStencilState depthStencilState = ((IPlatformDepthStencilState)_actualDepthStencilState).GetStrategy<ConcreteDepthStencilState>().GetDxState();
                this.D3dContext.OutputMerger.SetDepthStencilState(depthStencilState, _actualDepthStencilState.ReferenceStencil);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                D3D11.RasterizerState rasterizerState = ((IPlatformRasterizerState)_actualRasterizerState).GetStrategy<ConcreteRasterizerState>().GetDxState(this);
                this.D3dContext.Rasterizer.State = rasterizerState;
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
                D3D11.BlendState blendState = ((IPlatformBlendState)_actualBlendState).GetStrategy<ConcreteBlendState>().GetDxState();
                RawColor4 blendFactor = BlendFactor.ToRawColor4();
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

        internal void PlatformApplyViewport()
        {
            lock (this.SyncHandle)
            {
                if (this.D3dContext != null)
                {
                    RawViewportF viewport = _viewport.ToRawViewportF();
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
                    ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DXIndexBuffer,
                    ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType,
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
                        slot, new D3D11.VertexBufferBinding(((IPlatformVertexBuffer)vertexBuffer).Strategy.ToConcrete<ConcreteVertexBuffer>().DXVertexBuffer, vertexStride, vertexOffsetInBytes));
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

            ConcreteVertexShader cvertexShader = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();
            ConcretePixelShader  cpixelShader  = ((IPlatformShader)this.PixelShader).Strategy.ToConcrete<ConcretePixelShader>();

            if (_vertexShaderDirty)
            {
                this.D3dContext.VertexShader.Set(cvertexShader.DXVertexShader);

                base.Metrics_AddVertexShaderCount();
            }
            if (_vertexShaderDirty || _vertexBuffersDirty)
            {
                this.D3dContext.InputAssembler.InputLayout = cvertexShader.InputLayouts.GetOrCreate(_vertexBuffers);
                _vertexShaderDirty = false;
                _vertexBuffersDirty = false;
            }

            if (_pixelShaderDirty)
            {
                this.D3dContext.PixelShader.Set(cpixelShader.DXPixelShader);
                _pixelShaderDirty = false;

                base.Metrics_AddPixelShaderCount();
            }


            // Apply Constant Buffers
            ((IPlatformConstantBufferCollection)_vertexConstantBuffers).Strategy.ToConcrete<ConcreteConstantBufferCollection>().Apply(this, cvertexShader, this.D3dContext.VertexShader);
            ((IPlatformConstantBufferCollection)_pixelConstantBuffers).Strategy.ToConcrete<ConcreteConstantBufferCollection>().Apply(this, cpixelShader, this.D3dContext.PixelShader);

            // Apply Shader Texture and SamplersSamplers
            PlatformApplyTexturesAndSamplers(cvertexShader, this.D3dContext.VertexShader,
                ((IPlatformTextureCollection)this.VertexTextures).Strategy.ToConcrete<ConcreteTextureCollection>(),
                ((IPlatformSamplerStateCollection)this.VertexSamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>());
            PlatformApplyTexturesAndSamplers(cpixelShader, this.D3dContext.PixelShader,
                ((IPlatformTextureCollection)this.Textures).Strategy.ToConcrete<ConcreteTextureCollection>(),
                ((IPlatformSamplerStateCollection)this.SamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>());
        }

        private void PlatformApplyTexturesAndSamplers(ConcreteShader cshader, D3D11.CommonShaderStage dxShaderStage, ConcreteTextureCollection ctextureCollection, ConcreteSamplerStateCollection csamplerStateCollection)
        {
            // NOTE: We make the assumption here that the caller has locked the d3dContext for us to use.

            int texturesCount = ctextureCollection.Length;

            // Apply Textures
            for (int slot = 0; ctextureCollection.InternalDirty != 0 && slot < texturesCount; slot++)
            {
                uint mask = ((uint)1) << slot;
                if ((ctextureCollection.InternalDirty & mask) != 0)
                {
                    Texture texture = ctextureCollection[slot];

                    if (texture != null && !texture.IsDisposed)
                    {
                        ConcreteTexture ctexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>();
                        dxShaderStage.SetShaderResource(slot, ctexture.GetShaderResourceView());

                        this.Metrics_AddTextureCount();
                    }
                    else
                        dxShaderStage.SetShaderResource(slot, null);

                    // clear texture bit
                    ctextureCollection.InternalDirty &= ~mask;
                }
            }

            // Check Samplers
            if (((IPlatformGraphicsContext)this.Context).DeviceStrategy.GraphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < texturesCount; i++)
                {
                    Texture2D tx2D = ctextureCollection[i] as Texture2D;
                    if (tx2D != null)
                    {
                        if (this.SamplerStates[i].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Width)
                        || this.SamplerStates[i].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(tx2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            // Apply Samplers
            for (int slot = 0; csamplerStateCollection.InternalD3dDirty != 0 && slot < texturesCount; slot++)
            {
                uint mask = ((uint)1) << slot;
                if ((csamplerStateCollection.InternalD3dDirty & mask) != 0)
                {
                    D3D11.SamplerState state = null;

                    SamplerState sampler = csamplerStateCollection.InternalActualSamplers[slot];
                    if (sampler != null)
                    {
                        Debug.Assert(sampler.GraphicsDevice == ((IPlatformGraphicsContext)this.Context).DeviceStrategy.Device, "The state was created for a different device!");
               
                        ConcreteSamplerState csamplerState = ((IPlatformSamplerState)sampler).GetStrategy<ConcreteSamplerState>();

                        state = csamplerState.GetDxState();
                    }

                    dxShaderStage.SetSampler(slot, state);

                    // clear sampler bit
                    csamplerStateCollection.InternalD3dDirty &= ~mask;
                }
            }
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

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount, int vertexCount)
        {
            lock (this.SyncHandle)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.Draw(vertexCount, vertexStart);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            lock (this.SyncHandle)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
                this.D3dContext.DrawIndexed(indexCount, startIndex, baseVertex);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            lock (this.SyncHandle)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
                this.D3dContext.DrawIndexed(indexCount, startIndex, baseVertex);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex,
            int primitiveCount, int baseInstance, int instanceCount)
        {
            lock (this.SyncHandle)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers();
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);

                if (baseInstance > 0)
                {
                    if (!((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsBaseIndexInstancing)
                        throw new PlatformNotSupportedException("Instanced geometry drawing with base instance not supported.");

                    this.D3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, baseInstance);
                }
                else
                {
                    this.D3dContext.DrawIndexedInstanced(indexCount, instanceCount, startIndex, baseVertex, 0);
                }

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount * instanceCount);
            }
        }

        private int SetUserVertexBuffer<T>(T[] vertexData, int vertexOffset, int vertexCount, VertexDeclaration vertexDeclaration)
            where T : struct
        {
            UserVertexBuffer userVertexBuffer;
            if (!_userVertexBuffers.TryGetValue(vertexDeclaration, out userVertexBuffer))
            {
                userVertexBuffer = new UserVertexBuffer(vertexDeclaration.VertexStride);
                _userVertexBuffers[vertexDeclaration] = userVertexBuffer;
            }

            if (userVertexBuffer.Count < vertexCount)
            {
                if (userVertexBuffer.Buffer != null)
                    userVertexBuffer.Buffer.Dispose();

                int requiredVertexCount = Math.Max(vertexCount, 4 * 256);
                requiredVertexCount = (requiredVertexCount + 255) & (~255); // grow in chunks of 256.
                userVertexBuffer.Buffer = new DynamicVertexBuffer(((IPlatformGraphicsContext)this.Context).DeviceStrategy.Device, vertexDeclaration, requiredVertexCount, BufferUsage.WriteOnly);
                userVertexBuffer.Count = userVertexBuffer.Buffer.VertexCount;
            }

            int startVertex = userVertexBuffer.Offset;

            SetDataOptions setDataOptions = SetDataOptions.NoOverwrite;
            if ((startVertex + vertexCount) >= userVertexBuffer.Count)
            {
                setDataOptions = SetDataOptions.Discard;
                startVertex = 0;
            }

            userVertexBuffer.Buffer.SetData(startVertex * userVertexBuffer.Stride, vertexData, vertexOffset, vertexCount, userVertexBuffer.Stride, setDataOptions);
            userVertexBuffer.Offset = startVertex + vertexCount;

            SetVertexBuffer(userVertexBuffer.Buffer);
            return startVertex;
        }

        private int SetUserIndexBuffer<T>(UserIndexBuffer userIndexBuffer, T[] indexData, int indexOffset, int indexCount)
            where T : struct
        {
            Debug.Assert(indexCount >= 0);
            if (userIndexBuffer.Count < indexCount)
            {
                if (userIndexBuffer.Buffer != null)
                    userIndexBuffer.Buffer.Dispose();

                int requiredIndexCount = Math.Max(indexCount, 6 * 512);
                requiredIndexCount = (requiredIndexCount + 511) & (~511); // grow in chunks of 512.
                userIndexBuffer.Buffer = new DynamicIndexBuffer(((IPlatformGraphicsContext)this.Context).DeviceStrategy.Device, userIndexBuffer.ElementSize, requiredIndexCount, BufferUsage.WriteOnly);
                userIndexBuffer.Count = userIndexBuffer.Buffer.IndexCount;
            }

            int startIndex = userIndexBuffer.Offset;

            SetDataOptions setDataOptions = SetDataOptions.NoOverwrite;
            if ((startIndex + indexCount) >= userIndexBuffer.Count)
            {
                setDataOptions = SetDataOptions.Discard;
                startIndex = 0;
            }

            userIndexBuffer.Buffer.SetData<T>(startIndex * ((IPlatformIndexBuffer)userIndexBuffer.Buffer).Strategy.ElementSizeInBytes, indexData, indexOffset, indexCount, setDataOptions);
            userIndexBuffer.Offset = startIndex + indexCount;

            Indices = userIndexBuffer.Buffer;
            return startIndex;
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers.
            //       Bind directly to d3dContext and set dirty flags.
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, vertexCount, vertexDeclaration);

            lock (this.SyncHandle)
            {
                PlatformApplyState();
                //PlatformApplyIndexBuffer();
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.Draw(vertexCount, startVertex);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            int startIndex = SetUserIndexBuffer(_userIndexBuffer16, indexData, indexOffset, indexCount);

            lock (this.SyncHandle)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.DrawIndexed(indexCount, startIndex, startVertex);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            // TODO: Do not set public VertexBuffers and Indices.
            //       Bind directly to d3dContext and set dirty flags.
            int indexCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int startVertex = SetUserVertexBuffer(vertexData, vertexOffset, numVertices, vertexDeclaration);
            int startIndex = SetUserIndexBuffer(_userIndexBuffer32, indexData, indexOffset, indexCount);

            lock (this.SyncHandle)
            {
                PlatformApplyState();
                PlatformApplyIndexBuffer(); // SetUserIndexBuffer() overwrites the indexbuffer
                PlatformApplyVertexBuffers(); // SetUserVertexBuffer() overwrites the vertexBuffer
                PlatformApplyShaders();

                PlatformApplyPrimitiveType(primitiveType);
                this.D3dContext.DrawIndexed(indexCount, startIndex, startVertex);

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
        }

        public override void Flush()
        {
            this.D3dContext.Flush();
        }


        public override OcclusionQueryStrategy CreateOcclusionQueryStrategy()
        {
            return new ConcreteOcclusionQuery(this);
        }

        public override GraphicsDebugStrategy CreateGraphicsDebugStrategy()
        {
            return new ConcreteGraphicsDebug(this);
        }

        public override ConstantBufferCollectionStrategy CreateConstantBufferCollectionStrategy(int capacity)
        {
            return new ConcreteConstantBufferCollection(capacity);
        }

        public override TextureCollectionStrategy CreateTextureCollectionStrategy(int capacity)
        {
            return new ConcreteTextureCollection(this, capacity);
        }

        public override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(int capacity)
        {
            return new ConcreteSamplerStateCollection(this, capacity);
        }

        public override ITexture2DStrategy CreateTexture2DStrategy(int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
        {
            return new ConcreteTexture2D(this, width, height, mipMap, format, arraySize, shared);
        }

        public override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTexture3D(this, width, height, depth, mipMap, format);
        }

        public override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTextureCube(this, size, mipMap, format);
        }

        public override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTarget2D(this, width, height, mipMap, arraySize, shared, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount,
                                              surfaceType: TextureSurfaceType.RenderTarget);
        }

        public override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTarget3D(this, width, height, depth, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        public override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTargetCube(this, size, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        public override ITexture2DStrategy CreateTexture2DStrategy(Stream stream)
        {
            return new ConcreteTexture2D(this, stream);
        }

        public override ShaderStrategy CreateVertexShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcreteVertexShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        public override ShaderStrategy CreatePixelShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcretePixelShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        public override ConstantBufferStrategy CreateConstantBufferStrategy(string name, int[] parameterIndexes, int[] parameterOffsets, int sizeInBytes, ShaderProfileType profile)
        {
            return new ConcreteConstantBuffer(this, name, parameterIndexes, parameterOffsets, sizeInBytes, profile);
        }

        public override IndexBufferStrategy CreateIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        public override VertexBufferStrategy CreateVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteVertexBuffer(this, vertexDeclaration, vertexCount, usage);
        }
        public override IndexBufferStrategy CreateDynamicIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteDynamicIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        public override VertexBufferStrategy CreateDynamicVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteDynamicVertexBuffer(this, vertexDeclaration, vertexCount, usage);
        }

        public override IBlendStateStrategy CreateBlendStateStrategy(IBlendStateStrategy source)
        {
            return new ConcreteBlendState(this, source);
        }
        public override IDepthStencilStateStrategy CreateDepthStencilStateStrategy(IDepthStencilStateStrategy source)
        {
            return new ConcreteDepthStencilState(this, source);
        }
        public override IRasterizerStateStrategy CreateRasterizerStateStrategy(IRasterizerStateStrategy source)
        {
            return new ConcreteRasterizerState(this, source);
        }
        public override ISamplerStateStrategy CreateSamplerStateStrategy(ISamplerStateStrategy source)
        {
            return new ConcreteSamplerState(this, source);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userIndexBuffer16.Buffer != null)
                    _userIndexBuffer16.Buffer.Dispose();
                _userIndexBuffer16.Buffer = null;
                if (_userIndexBuffer32.Buffer != null)
                    _userIndexBuffer32.Buffer.Dispose();
                _userIndexBuffer32.Buffer = null;

                foreach (UserVertexBuffer uvb in _userVertexBuffers.Values)
                {
                    uvb.Buffer.Dispose();
                    uvb.Buffer = null;
                }

                if (_d3dContext != null)
                    _d3dContext.Dispose();
                _d3dContext = null;
            }

            base.Dispose(disposing);
        }

        protected override void PlatformResolveRenderTargets()
        {
            for (int i = 0; i < base.RenderTargetCount; i++)
            {
                RenderTargetBinding renderTargetBinding = base.CurrentRenderTargetBindings[i];

                // Resolve MSAA render targets
                RenderTarget2D renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget != null && renderTarget.MultiSampleCount > 1)
                {
                    ((IRenderTarget)renderTarget).RenderTargetStrategy.ResolveSubresource(this);
                }

                // Generate mipmaps.
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    lock (this.SyncHandle)
                    {
                        this.D3dContext.GenerateMips(((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ConcreteTexture>().GetShaderResourceView());
                    }
                }
            }
        }

        protected override void PlatformApplyDefaultRenderTarget()
        {
            // Set the default swap chain.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentRenderTargets[0] = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._renderTargetView;
            _currentDepthStencilView = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._depthStencilView;

            lock (this.SyncHandle)
            {
                this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
            }
        }

        protected override IRenderTarget PlatformApplyRenderTargets()
        {
            // Clear the current render targets.
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            _currentDepthStencilView = null;

            // Make sure none of the new targets are bound
            // to the device as a texture resource.
            lock (this.SyncHandle)
            {
                ((IPlatformTextureCollection)this.VertexTextures).Strategy.ToConcrete<ConcreteTextureCollection>().ClearTargets(base.CurrentRenderTargetBindings, this.D3dContext.VertexShader);
                ((IPlatformTextureCollection)this.Textures).Strategy.ToConcrete<ConcreteTextureCollection>().ClearTargets(base.CurrentRenderTargetBindings, this.D3dContext.PixelShader);
            }

            for (int i = 0; i < base.RenderTargetCount; i++)
            {
                RenderTargetBinding renderTargetBinding = base.CurrentRenderTargetBindings[i];
                IRenderTargetStrategyDX11 targetDX = (IRenderTargetStrategyDX11)((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>();
                _currentRenderTargets[i] = targetDX.GetRenderTargetView(renderTargetBinding.ArraySlice);
            }

            // Use the depth from the first target.
            IRenderTargetStrategyDX11 renderTargetDX = (IRenderTargetStrategyDX11)((IPlatformTexture)base.CurrentRenderTargetBindings[0].RenderTarget).GetTextureStrategy<ITextureStrategy>();
            _currentDepthStencilView = renderTargetDX.GetDepthStencilView(base.CurrentRenderTargetBindings[0].ArraySlice);

            // Set the targets.
            lock (this.SyncHandle)
            {
                this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);
            }

            return (IRenderTarget)base.CurrentRenderTargetBindings[0].RenderTarget;
        }

        internal void ClearCurrentRenderTargets()
        {
            _currentDepthStencilView = null;
            Array.Clear(_currentRenderTargets, 0, _currentRenderTargets.Length);
            Array.Clear(base.CurrentRenderTargetBindings, 0, base.CurrentRenderTargetBindings.Length);
            base._currentRenderTargetCount = 0;
        }


#if UAP || WINUI
        internal void UAP_ResetRenderTargets()
        {
            PlatformApplyViewport();
                        
            lock (this.SyncHandle)
                    this.D3dContext.OutputMerger.SetTargets(_currentDepthStencilView, _currentRenderTargets);

            ((IPlatformTextureCollection)this.Textures).Strategy.Dirty();
            ((IPlatformSamplerStateCollection)this.SamplerStates).Strategy.Dirty();
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
