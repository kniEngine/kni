// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicVertexBuffer : VertexBuffer
    {
        public bool IsContentLost
        {
            get
            {
                throw new NotImplementedException("IsContentLost");
            }
        }

		
		public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage usage)
            : this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, usage)
        {
        }

        public DynamicVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(graphicsDevice, vertexDeclaration, vertexCount, usage, true)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            _strategy = graphicsDevice.CurrentContext.Strategy.CreateDynamicVertexBufferStrategy(vertexDeclaration, vertexCount, usage);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
            base.SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, options);
        }
    }
}

