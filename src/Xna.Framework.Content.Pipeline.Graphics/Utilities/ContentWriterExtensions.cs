// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public static class ContentWriterExtensions
    {
        /// <summary>
        ///  Writes a Color value to the current stream and advances the stream position by 4 bytes.
        /// </summary>
        /// <param name="output">The ContentWriter.</param>
        /// <param name="value">The Color value to write.</param>
        public static void Write(this ContentWriter output, Color value)
        {
            output.Write(value.R);
            output.Write(value.G);
            output.Write(value.B);
            output.Write(value.A);
        }
    }
}
