// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    class VertexBufferReader : ContentTypeReader<VertexBuffer>
    {
        protected internal override VertexBuffer Read(ContentReader input, VertexBuffer existingInstance)
        {
            VertexDeclaration declaration = input.ReadRawObject<VertexDeclaration>();
            int vertexCount = (int)input.ReadUInt32();
            int dataSize = vertexCount * declaration.VertexStride;
            byte[] data = ContentBufferPool.Current.Get(dataSize);
            input.Read(data, 0, dataSize);

            VertexBuffer buffer = existingInstance ?? new VertexBuffer(input.GetGraphicsDevice(), declaration, vertexCount, BufferUsage.None);
            buffer.SetData(data, 0, dataSize);
            ContentBufferPool.Current.Return(data);
            return buffer;
        }
    }
}
