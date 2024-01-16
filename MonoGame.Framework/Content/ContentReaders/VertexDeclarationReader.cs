// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class VertexDeclarationReader : ContentTypeReader<VertexDeclaration>
    {
        protected internal override VertexDeclaration Read(ContentReader input, VertexDeclaration existingInstance)
        {
            int vertexStride = input.ReadInt32();
            int elementCount = input.ReadInt32();

            VertexElement[] elements = new VertexElement[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                int offset = input.ReadInt32();
                VertexElementFormat elementFormat = (VertexElementFormat)input.ReadInt32();
                VertexElementUsage elementUsage = (VertexElementUsage)input.ReadInt32();
                int usageIndex = input.ReadInt32();
                elements[i] = new VertexElement(offset, elementFormat, elementUsage, usageIndex);
            }

            VertexDeclaration vertexDeclaration = VertexDeclaration.GetOrCreate(vertexStride, elements);

            return vertexDeclaration;
        }
    }
}
