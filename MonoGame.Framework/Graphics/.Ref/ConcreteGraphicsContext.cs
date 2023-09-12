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

        internal void PlatformResolveRenderTargets()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApplyDefaultRenderTarget()
        {
            throw new PlatformNotSupportedException();
        }

        internal IRenderTarget PlatformApplyRenderTargets()
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
            throw new NotImplementedException();
        }

        internal override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        internal override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        internal override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, Texture2D.SurfaceType surfaceType)
        {
            throw new NotImplementedException();
        }

        internal override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        internal override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        internal override ITexture2DStrategy CreateTexture2DStrategy(Stream stream)
        {
            return new ConcreteTexture2D(this, stream);
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
