// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    public class SpriteFontWriter : ContentTypeWriter<SpriteFontContent>
    {
        protected override void Write(ContentWriter output, SpriteFontContent value)
        {
            output.WriteObject(value.Texture);
            output.WriteObject(value.Glyphs);
            output.WriteObject(value.Cropping);
            output.WriteObject(value.CharacterMap);
            output.Write(value.VerticalLineSpacing);
            output.Write(value.HorizontalSpacing);
            output.WriteObject(value.Kerning);
            var hasDefChar = value.DefaultCharacter.HasValue;
            output.Write(hasDefChar);
            if (hasDefChar)
                output.Write(value.DefaultCharacter.Value);
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime loader for this type.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform.</param>
        /// <returns>Name of the runtime loader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.SpriteFontReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime target type. The runtime target type often matches the design time type, but may differ.
        /// </summary>
        /// <param name="targetPlatform">The target platform.</param>
        /// <returns>The qualified name.</returns>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            // Base the reader type string from a known public class in the same namespace in the same assembly
            Type type = typeof(ContentReader);
			string readerType = type.Namespace + ".SpriteFontReader, " + type.AssemblyQualifiedName;
            return readerType;
        }

    }
}
