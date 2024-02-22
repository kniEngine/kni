// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class VideoWriter : ContentTypeWriter<VideoContent>
    {
        protected override void Write(ContentWriter output, VideoContent value)
        {
            output.WriteObject<string>(value.Filename);
            output.WriteObject<int>((int)value.Duration.TotalMilliseconds);
            output.WriteObject<int>(value.Width);
            output.WriteObject<int>(value.Height);
            output.WriteObject<float>(value.FramesPerSecond);
            output.WriteObject<int>((int)value.VideoSoundtrackType);
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            string readerName = ".VideoReader";
            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            string readerAssembly = String.Empty;

            string runtimeReader = readerNamespace + readerName + readerAssembly;
            return runtimeReader;
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeNamespace = "Microsoft.Xna.Framework.Media";
            string typeName = ".Video";
            string typeAssembly = ", Microsoft.Xna.Framework.Video, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            string runtimeType = typeNamespace + typeName + typeAssembly;
            return runtimeType;
        }
    }
}
