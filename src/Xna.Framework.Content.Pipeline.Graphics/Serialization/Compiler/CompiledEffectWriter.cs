// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class CompiledEffectWriter : ContentTypeWriterBase<CompiledEffectContent>
    {
        protected override void Write(ContentWriter output, CompiledEffectContent value)
        {
            byte[] bytecode = value.GetEffectCode();

            output.Write(bytecode.Length);
            output.Write(bytecode);
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            string readerName = ".EffectReader";
            string readerAssembly = ", Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            string runtimeReader = readerNamespace + readerName + readerAssembly;
            return runtimeReader;
        }
    }
}
