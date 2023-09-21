// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicIndexBuffer : IndexBuffer
	{

        public bool IsContentLost
        {
            get { throw new NotImplementedException("IsContentLost"); }
        }

   		public DynamicIndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage)
        {
        }

        public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage) :
			base(graphicsDevice, indexElementSize, indexCount, usage, true)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.Strategy.GraphicsProfile == GraphicsProfile.Reach && indexElementSize == IndexElementSize.ThirtyTwoBits)
                throw new NotSupportedException("Reach profile does not support 32 bit indices");

            _strategy = graphicsDevice.CurrentContext.Strategy.CreateDynamicIndexBufferStrategy(indexElementSize, indexCount, usage);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }


        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, options);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(0, data, startIndex, elementCount, options);
        }
    }
}

