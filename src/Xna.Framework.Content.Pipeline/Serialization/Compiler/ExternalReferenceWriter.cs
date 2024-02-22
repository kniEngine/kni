// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the external reference to the output.
    /// </summary>
    [ContentTypeWriter]
    class ExternalReferenceWriter<T> : ContentTypeWriterBaseGeneric<ExternalReference<T>>
    {
        private ContentTypeWriter _targetWriter;

        protected internal override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);
        }

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);
            _targetWriter = output.GetTypeWriter(typeof(T));
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected override void Write(ContentWriter output, ExternalReference<T> value)
        {
            output.WriteExternalReference(value);
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            string readerName = ".ExternalReferenceReader";
            string readerAssembly = ", " + typeof(ContentReader).Assembly.FullName;

            string runtimeReader = readerNamespace + readerName + readerAssembly;
            return runtimeReader;
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string runtimeType = _targetWriter.GetRuntimeType(targetPlatform);
            return runtimeType;
        }
    }
}
