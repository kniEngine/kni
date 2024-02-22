// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class IndexBufferWriter : ContentTypeWriterBase<IndexCollection>
    {
        protected override void Write(ContentWriter output, IndexCollection value)
        {
            // Check if the buffer and can be saved as Int16.
            bool shortIndices = true;
            foreach(int index in value)
            {
                if(index > ushort.MaxValue)
                {
                    shortIndices = false;
                    break;
                }
            }

            output.Write(shortIndices);

            int indicesSize = shortIndices ? 2 : 4;
            int byteCount = value.Count * indicesSize;

            output.Write(byteCount);
            if (shortIndices)
            {
                foreach (int item in value)
                    output.Write((ushort)item);
            }
            else
            {
                foreach (int item in value)
                    output.Write(item);
            }
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.IndexBufferReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            Type type = typeof(ContentReader);

            string readerType = type.Namespace + ".IndexBufferReader, " + type.AssemblyQualifiedName;
            return readerType;
        }
    }
}
