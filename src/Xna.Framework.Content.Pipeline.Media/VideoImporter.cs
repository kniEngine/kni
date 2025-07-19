// Copyright (C)2025 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [ContentImporter(".wmv",
                     ".asf",
                     ".mp4",
                     ".m4v",
                     ".avi",
                     ".divx",
                     ".xvid",
                     ".mov",
                     ".mpg",
                     ".mpeg",
                     ".webm",
                     ".mkv",
                     ".ogv",
                     ".mjpeg",
                     ".vob",
                     ".ts",
                     ".y4m",
                     ".flv",
                     ".3gp",
                     DisplayName = "Video Importer - KNI", DefaultProcessor = "VideoProcessor")]
    public class VideoImporter : ContentImporter<VideoContent>
    {
        public VideoImporter()
        {
        }

        public override VideoContent Import(string filename, ContentImporterContext context)
        {
            var content = new VideoContent(filename);
            return content;
        }
    }
}
