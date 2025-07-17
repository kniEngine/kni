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

            VideoContent output = ConvertFormat(input, context, VideoFormat);
            absVideoPath = Path.ChangeExtension(absVideoPath, Path.GetExtension(output.Filename));

            // Copy the already encoded video file over
            File.Copy(output.Filename, absVideoPath, true);
            context.AddOutputFile(absVideoPath);

            // Fixup relative path
            string relativeMediaPath = PathHelper.GetRelativePath(context.OutputFilename, absVideoPath);
            output.Filename = relativeMediaPath;

            return output;
        }

        private VideoContent ConvertFormat(VideoContent input, ContentProcessorContext context, VideoProcessorOutputFormat videoFormat)
        {
            if (videoFormat == VideoProcessorOutputFormat.NoChange)
            {
                return input;
            }
            else
            {
                if (videoFormat == VideoProcessorOutputFormat.Default)
                    videoFormat = VideoProcessor.GetDefaultOutputFormat(context.TargetPlatform);

                string tmpPath = Path.GetTempPath();
                string tmpFilename = Path.GetRandomFileName();
                string containerName = VideoProcessor.GetExtension(videoFormat);
                string saveToFile = Path.Combine(tmpPath, tmpFilename + "." + containerName);

                switch (videoFormat)
                {
                    case VideoProcessorOutputFormat.WMV:
                            return ConvertToWmv(input, saveToFile);
                    case VideoProcessorOutputFormat.MP4:
                            return ConvertToMP4(input, saveToFile);
                    case VideoProcessorOutputFormat.WebM:
                            return ConvertToWebM(input, saveToFile);

                    default:
                        throw new InvalidOperationException("Unsupported video format: " + videoFormat);
                }
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
                        return "wmv";
                case VideoProcessorOutputFormat.MP4:
                        return "mp4";
                case VideoProcessorOutputFormat.WebM:
                        return "webm";

                default:
                    throw new InvalidOperationException("Unsupported video format: " + videoFormat);
            }
        }

        private VideoContent ConvertToWmv(VideoContent input, string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "wmv2";
            ffmpegACodecName = "wmav2";

            string args = string.Format(
                    "-y -i \"{0}\" -c:v {1} -c:a {2} -movflags +faststart \"{3}\"",
                    input.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);

            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);

            return new VideoContent(saveToFile);
        }

        private VideoContent ConvertToMP4(VideoContent input, string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "libx264";
            ffmpegACodecName = "aac";

            string args = string.Format(
                    //"-y -i \"{0}\" -c:v {1} -profile:v baseline -level 3.0 -c:a {2} -strict -2 -movflags +faststart \"{3}\"",
                    "-y -i \"{0}\" -c:v {1} -profile:v main -c:a {2} -strict -2 -movflags +faststart \"{3}\"",
                    input.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);

            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);

            return new VideoContent(saveToFile);
        }

        private VideoContent ConvertToWebM(VideoContent input, string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "libvpx-vp9";
            ffmpegACodecName = "libopus";

            string args = string.Format(
                    "-y -i \"{0}\" -c:v {1} -b:v 0 -crf 30 -c:a {2} -movflags +faststart -fflags +bitexact \"{3}\"",
                    input.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);            
            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);

            return new VideoContent(saveToFile);
        }
    }
}
