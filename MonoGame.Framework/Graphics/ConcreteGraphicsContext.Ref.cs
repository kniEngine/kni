// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
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


        internal override GraphicsDebugStrategy CreateGraphicsDebugStrategy(GraphicsContextStrategy contextStrategy)
        {
            return new ConcreteGraphicsDebug(contextStrategy);
        }

        internal override TextureCollectionStrategy CreateTextureCollectionStrategy(GraphicsContextStrategy contextStrategy, int capacity)
        {
            return new ConcreteTextureCollection(contextStrategy, capacity);
        }

        internal override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(GraphicsContextStrategy contextStrategy, int capacity)
        {
            return new ConcreteSamplerStateCollection(contextStrategy, capacity);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            base.Dispose(disposing);
        }

    }
}
