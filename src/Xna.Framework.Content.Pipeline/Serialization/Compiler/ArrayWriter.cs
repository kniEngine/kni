// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the array value to the output.
    /// </summary>
    [ContentTypeWriter]
    class ArrayWriter<T> : ContentTypeWriterBaseGeneric<T[]>
    {
        ContentTypeWriter _elementWriter;

        protected internal override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);
        }

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            _elementWriter = output.GetTypeWriter(typeof(T));
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return string.Concat(   "Microsoft.Xna.Framework.Content.",
                                    "ArrayReader`1[[", 
                                    _elementWriter.GetRuntimeType(targetPlatform), 
                                    "]]");
        }

        protected override void Write(ContentWriter output, T[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            output.Write(value.Length);
            foreach (T element in value)
                output.WriteObject(element, _elementWriter);
        }
    }
}
