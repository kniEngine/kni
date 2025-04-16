// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using TOutput = Microsoft.Xna.Framework.Color;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the Color value to the output.
    /// </summary>
    [ContentTypeWriter]
    class ColorWriter : ContentTypeWriterBase<TOutput>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected override void Write(ContentWriter output, TOutput value)
        {
            output.Write(value.R);
            output.Write(value.G);
            output.Write(value.B);
            output.Write(value.A);
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeNamespace = "Microsoft.Xna.Framework"; // Color namespace is not 'Microsoft.Xna.Framework.Graphics'
            string typeName = TargetType.Name;
            string typeAssembly = TargetType.Assembly.FullName;

            if (typeAssembly.StartsWith("Xna.Framework.Graphics,"))
                typeAssembly = "Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            string runtimeType = typeNamespace + "." + typeName + ", " + typeAssembly;
            return runtimeType;
        }
    }
}
