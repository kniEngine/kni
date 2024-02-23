// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Video - KNI")]
    public class VideoProcessor : ContentProcessor<VideoContent, VideoContent>
    {
        public override VideoContent Process(VideoContent input, ContentProcessorContext context)
        {
            string relative = Path.GetDirectoryName(PathHelper.GetRelativePath(context.OutputDirectory, context.OutputFilename));
            string relVideoPath = PathHelper.Normalize(Path.Combine(relative, Path.GetFileName(input.Filename)));
            string absVideoPath = PathHelper.Normalize(Path.Combine(context.OutputDirectory, relVideoPath));

            // Make sure the output folder for the video exists.
            Directory.CreateDirectory(Path.GetDirectoryName(absVideoPath));

            // Copy the already encoded video file over
            File.Copy(input.Filename, absVideoPath, true);
            context.AddOutputFile(absVideoPath);

            // Fixup relative path
            string relativeMediaPath = PathHelper.GetRelativePath(context.OutputFilename, absVideoPath);
            input.Filename = relativeMediaPath;

            return input;
        }
    }
}
