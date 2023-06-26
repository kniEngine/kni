// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {

        private void PlatformSetup()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }        

        private void PlatformDispose()
        {
        }

        internal void PlatformPresent()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {            
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }        

        internal void OnPresentationChanged()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
