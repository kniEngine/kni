// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2025 Nick Kastellanos

using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Video - KNI")]
    public class VideoProcessor : ContentProcessor<VideoContent, VideoContent>
    {

        [DefaultValue(typeof(VideoProcessorOutputFormat), "Default")]
        public virtual VideoProcessorOutputFormat VideoFormat { get; set; }


        public override VideoContent Process(VideoContent input, ContentProcessorContext context)
        {
            string relative = Path.GetDirectoryName(PathHelper.GetRelativePath(context.OutputDirectory, context.OutputFilename));
            string relVideoPath = PathHelper.Normalize(Path.Combine(relative, Path.GetFileName(input.Filename)));
            string absVideoPath = PathHelper.Normalize(Path.Combine(context.OutputDirectory, relVideoPath));

            // Make sure the output folder for the video exists.
            Directory.CreateDirectory(Path.GetDirectoryName(absVideoPath));

            VideoContent output;
            VideoProcessorOutputFormat videoFormat = VideoFormat;

            if (videoFormat == VideoProcessorOutputFormat.NoChange)
            {
                output = input;
            }
            else
            {
                if (videoFormat == VideoProcessorOutputFormat.Default)
                    videoFormat = VideoProcessor.GetDefaultOutputFormat(context.TargetPlatform);

                string containerExt = VideoProcessor.GetExtension(videoFormat);

                string tmpPath = Path.GetTempPath();
                string tmpFilename = Path.GetRandomFileName();
                string saveToFile = Path.Combine(tmpPath, tmpFilename + containerExt);

                output = ConvertFormat(input, saveToFile);
                absVideoPath = Path.ChangeExtension(absVideoPath, containerExt);
            }

            // Copy the already encoded video file over
            File.Copy(output.Filename, absVideoPath, true);
            context.AddOutputFile(absVideoPath);

            // Fixup relative path
            string relativeMediaPath = PathHelper.GetRelativePath(context.OutputFilename, absVideoPath);
            output.Filename = relativeMediaPath;

            return output;
        }

        private VideoContent ConvertFormat(VideoContent input, string saveToFile)
        {
            string containerExt = Path.GetExtension(saveToFile).ToLower();

            switch (containerExt)
            {
                case ".wmv":
                    return input.ConvertToWmv(saveToFile);
                case ".mp4":
                    return input.ConvertToMP4(saveToFile);
                case ".webm":
                    return input.ConvertToWebM(saveToFile);

                default:
                    throw new InvalidOperationException("Unsupported video format: " + containerExt);
            }
        }

        private static VideoProcessorOutputFormat GetDefaultOutputFormat(TargetPlatform targetPlatform)
        {
            switch (targetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsStoreApp:
                    return VideoProcessorOutputFormat.WMV;
                case TargetPlatform.iOS:
                    return VideoProcessorOutputFormat.MP4;
                case TargetPlatform.Android:
                    return VideoProcessorOutputFormat.MP4;
                case TargetPlatform.BlazorGL:
                    return VideoProcessorOutputFormat.MP4;
                case TargetPlatform.DesktopGL:
                    return VideoProcessorOutputFormat.MP4;

                default:
                    return VideoProcessorOutputFormat.MP4;
            }
        }

        private static string GetExtension(VideoProcessorOutputFormat videoFormat)
        {
            switch (videoFormat)
            {
                case VideoProcessorOutputFormat.WMV:
                        return ".wmv";
                case VideoProcessorOutputFormat.MP4:
                        return ".mp4";
                case VideoProcessorOutputFormat.WebM:
                        return ".webm";

                default:
                    throw new InvalidOperationException("Unsupported video format: " + videoFormat);
            }
        }
    }
}
