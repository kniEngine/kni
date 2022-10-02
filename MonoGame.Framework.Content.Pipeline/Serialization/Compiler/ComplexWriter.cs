// Copyright (C)2022 Nick Kastellanos

using System;
using TOutput = Microsoft.Xna.Framework.Complex;


namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the Complex value to the output.
    /// </summary>
    [ContentTypeWriter]
    class ComplexWriter : BuiltInContentWriter<TOutput>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, TOutput value)
        {
            output.Write(value);
        }
    }
}
