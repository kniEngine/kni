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
    internal abstract class ContentTypeWriterBase<T> : ContentTypeWriter<T>
    {
        /// <summary>
        /// Gets the assembly qualified name of the runtime loader for this type.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform.</param>
        /// <returns>Name of the runtime loader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Change "Writer" in this class name to "Reader" and use the runtime type namespace and assembly
            string readerClassName = this.GetType().Name.Replace("Writer", "Reader");

            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            return "Microsoft.Xna.Framework.Content." + readerClassName;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeName = TargetType.FullName;
            string asmName = TargetType.Assembly.FullName;

            if (asmName.StartsWith("Xna.Framework,"))
                asmName = "Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
            if (asmName.StartsWith("MonoGame.Framework,"))
                asmName = "Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            return typeName + ", " + asmName;
        }
    }
}
