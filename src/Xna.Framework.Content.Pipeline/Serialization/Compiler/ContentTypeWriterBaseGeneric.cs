// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Base class for the built-in content type writers where the content type is the same as the runtime type.
    /// </summary>
    /// <typeparam name="T">The content type being written.</typeparam>
    internal abstract class ContentTypeWriterBaseGeneric<T> : ContentTypeWriter<T>
    {
        private List<ContentTypeWriter> _genericArgWriters;

        protected internal override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);
        }

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            _genericArgWriters = new List<ContentTypeWriter>();

            Type[] arguments = TargetType.GetGenericArguments();
            foreach (Type argType in arguments)
            {
                ContentTypeWriter argWriter = output.GetTypeWriter(argType);
                _genericArgWriters.Add(argWriter);
            }
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            // Change "Writer" in this class name to "Reader" and use the runtime type namespace and assembly
            string readerName = this.GetType().Name.Replace("Writer", "Reader");

            // Add generic arguments
            readerName += "[";
            foreach (ContentTypeWriter argWriter in _genericArgWriters)
            {
                readerName += "[";
                readerName += argWriter.GetRuntimeType(targetPlatform);
                readerName += "]";
                // Important: Do not add a space char after the comma because 
                // this will not work with Type.GetType in Xamarin.Android!
                readerName += ",";
            }
            readerName = readerName.TrimEnd(',', ' ');
            readerName += "]";

            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            string readerAssembly = String.Empty;

            string runtimeReader = readerNamespace + "." + readerName + readerAssembly;
            return runtimeReader;
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeFullName = TargetType.FullName;
            string typeAssembly = TargetType.Assembly.FullName;

            if (typeAssembly.StartsWith("MonoGame.Framework,"))
                typeAssembly = "Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            string runtimeType = typeFullName + ", " + typeAssembly;
            return runtimeType;
        }
    }
}
