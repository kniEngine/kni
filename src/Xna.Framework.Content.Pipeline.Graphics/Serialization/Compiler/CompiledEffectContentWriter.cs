// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class CompiledEffectContentWriter : ContentTypeWriterBase<CompiledEffectContent>
    {
        protected override void Write(ContentWriter output, CompiledEffectContent value)
        {
            byte[] code = value.GetEffectCode();

            output.Write(code.Length);
            output.Write(code);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            Type type = typeof(ContentReader);

            string readerType = type.Namespace + ".EffectReader, " + type.Assembly.FullName;
            return readerType;
        }
    }
}
