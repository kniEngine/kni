﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a base class for all video objects.
    /// </summary>
    public class VideoContent : ContentItem, IDisposable
    {
        private bool _disposed;
        private int _bitsPerSecond;
        private TimeSpan _duration;
        private float _framesPerSecond;
        private int _height;
        private int _width;

        /// <summary>
        /// Gets the bit rate for this video.
        /// </summary>
        public int BitsPerSecond { get { return _bitsPerSecond; } }

        /// <summary>
        /// Gets the duration of this video.
        /// </summary>
        public TimeSpan Duration { get { return _duration; } }

        /// <summary>
        /// Gets or sets the file name for this video.
        /// </summary>
        [ContentSerializerAttribute]
        public string Filename { get; set; }

        /// <summary>
        /// Gets the frame rate for this video.
        /// </summary>
        public float FramesPerSecond { get { return _framesPerSecond; } }

        /// <summary>
        /// Gets the height of this video.
        /// </summary>
        public int Height { get { return _height; } }

        /// <summary>
        /// Gets or sets the type of soundtrack accompanying the video.
        /// </summary>
        [ContentSerializerAttribute]
        public VideoSoundtrackType VideoSoundtrackType { get; set; }

        /// <summary>
        /// Gets the width of this video.
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Initializes a new copy of the VideoContent class for the specified video file.
        /// </summary>
        /// <param name="filename">The file name of the video to import.</param>
        public VideoContent(string filename)
        {
            Filename = filename;

            VideoContent.ProbeFormat(filename, out _width, out _height, out _duration, out _bitsPerSecond, out _framesPerSecond);
        }

        private static void ProbeFormat(string sourceFile, out int width, out int height, out TimeSpan duration, out int bitsPerSecond, out float framesPerSecond)
        {
            // Set default values if information is not available.
            width = 0;
            height = 0;
            duration = TimeSpan.Zero;
            bitsPerSecond = 0;
            framesPerSecond = 0.0f;

            string stdout, stderr;
            var result = ExternalTool.Run("ffprobe",
                string.Format("-i \"{0}\" -show_format -select_streams v -show_streams -print_format ini", sourceFile), out stdout, out stderr);

            var lines = stdout.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (!line.Contains('='))
                    continue;

                var key = line.Substring(0, line.IndexOf('='));
                var value = line.Substring(line.IndexOf('=') + 1);
                switch (key)
                {
                    case "duration":
                        if (value != "N/A")
                            duration = TimeSpan.FromSeconds(double.Parse(value, CultureInfo.InvariantCulture));
                        break;

                    case "bit_rate":
                        if (value != "N/A")
                            bitsPerSecond = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "width":
                        width = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "height":
                        height = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "r_frame_rate":
                        var frac = value.Split('/');
                        framesPerSecond = float.Parse(frac[0], CultureInfo.InvariantCulture) / float.Parse(frac[1], CultureInfo.InvariantCulture);
                        break;
                }
            }
        }

        public void ConvertFormat(string saveToFile)
        {
            string containerExt = Path.GetExtension(saveToFile).ToLower();

            switch (containerExt)
            {
                case ".wmv":
                    this.ConvertToWmv(saveToFile);
                    VideoContent.ProbeFormat(saveToFile, out _width, out _height, out _duration, out _bitsPerSecond, out _framesPerSecond);
                    this.Filename = saveToFile;
                    break;
                case ".mp4":
                    this.ConvertToMP4(saveToFile);
                    VideoContent.ProbeFormat(saveToFile, out _width, out _height, out _duration, out _bitsPerSecond, out _framesPerSecond);
                    this.Filename = saveToFile;
                    break;
                case ".webm":
                    this.ConvertToWebM(saveToFile);
                    VideoContent.ProbeFormat(saveToFile, out _width, out _height, out _duration, out _bitsPerSecond, out _framesPerSecond);
                    this.Filename = saveToFile;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported video format: " + containerExt);
            }
        }

        private void ConvertToWmv(string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "wmv2";
            ffmpegACodecName = "wmav2";

            string args = string.Format(
                    "-y -i \"{0}\" -c:v {1} -c:a {2} -movflags +faststart \"{3}\"",
                    this.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);

            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
        }

        private void ConvertToMP4(string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "libx264";
            ffmpegACodecName = "aac";

            string args = string.Format(
                    //"-y -i \"{0}\" -c:v {1} -profile:v baseline -level 3.0 -c:a {2} -strict -2 -movflags +faststart \"{3}\"",
                    "-y -i \"{0}\" -c:v {1} -profile:v main -c:a {2} -strict -2 -movflags +faststart \"{3}\"",
                    this.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);

            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
        }

        private void ConvertToWebM(string saveToFile)
        {
            string ffmpegVCodecName, ffmpegACodecName;
            ffmpegVCodecName = "libvpx-vp9";
            ffmpegACodecName = "libopus";

            string args = string.Format(
                    "-y -i \"{0}\" -c:v {1} -b:v 0 -crf 30 -c:a {2} -fflags +bitexact \"{3}\"",
                    this.Filename,
                    ffmpegVCodecName,
                    ffmpegACodecName,
                    saveToFile);

            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode;

            ffmpegExitCode = ExternalTool.Run("ffmpeg", args, out ffmpegStdout, out ffmpegStderr);
            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
        }

        ~VideoContent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: Free managed resources here
                    // ...
                }
                // TODO: Free unmanaged resources here
                // ...
                _disposed = true;
            }
        }
    }
}
