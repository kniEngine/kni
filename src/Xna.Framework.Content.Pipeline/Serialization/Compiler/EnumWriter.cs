// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the enum value to the output. Usually 32 bit, but can be other sizes if T is not integer.
    /// </summary>
    /// <typeparam name="T">The enum type to write.</typeparam>
    [ContentTypeWriter]
    class EnumWriter<T> : ContentTypeWriterBaseGeneric<T>
    {
        Type _underlyingType;
        ContentTypeWriter _underlyingTypeWriter;

        protected internal override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);
        }

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);
            _underlyingType = Enum.GetUnderlyingType(typeof(T));
            _underlyingTypeWriter = output.GetTypeWriter(_underlyingType);
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            string readerName = ".EnumReader`1"
                              + "[["
                              + GetRuntimeType(targetPlatform)
                              + "]]"
                              ;
            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            string readerAssembly = String.Empty;

            string runtimeReader = readerNamespace + readerName + readerAssembly;
            return runtimeReader;
        }

        protected override void Write(ContentWriter output, T value)
        {
            output.WriteRawObject(Convert.ChangeType(value, _underlyingType), _underlyingTypeWriter);
        }
    }
}
