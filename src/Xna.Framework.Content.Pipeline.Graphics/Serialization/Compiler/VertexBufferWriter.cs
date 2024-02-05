// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class VertexBufferWriter : ContentTypeWriterBase<VertexBufferContent>
    {
        protected override void Write(ContentWriter output, VertexBufferContent value)
        {
            output.WriteRawObject(value.VertexDeclaration);

            uint vertexCount = (uint)(value.VertexData.Length / value.VertexDeclaration.VertexStride);

            int rem = (value.VertexData.Length % value.VertexDeclaration.VertexStride.Value);
            if (rem != 0)
                throw new PipelineException("Invalid VertexData.Length");

            output.Write((uint)vertexCount);
            output.Write(value.VertexData);
        }
    }
}
