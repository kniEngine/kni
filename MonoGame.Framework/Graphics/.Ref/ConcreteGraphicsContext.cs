// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {

        }

        public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApplyViewport()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformResolveRenderTargets()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformApplyDefaultRenderTarget()
        {
            throw new PlatformNotSupportedException();
        }

        protected override IRenderTarget PlatformApplyRenderTargets()
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        public override void Flush()
        {
            throw new PlatformNotSupportedException();
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
            throw new NotImplementedException();
        }

        public override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        public override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        public override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        public override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        public override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            throw new NotImplementedException();
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
            }

            base.Dispose(disposing);
        }

    }
}
