// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A custom song processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a Song type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Song - KNI")]
    public class SongProcessor : ContentProcessor<AudioContent, SongContent>
    {
        ConversionQuality _quality = ConversionQuality.Best;

        /// <summary>
        /// Gets or sets the target format quality of the audio content.
        /// </summary>
        /// <value>The ConversionQuality of this audio data.</value>
        public ConversionQuality Quality 
        { 
            get { return _quality; } 
            set { _quality = value; } 
        }

        /// <summary>
        /// Initializes a new instance of SongProcessor.
        /// </summary>
        public SongProcessor()
        {
        }

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            // The xnb name is the basis for the final song filename.
            string songFileName = context.OutputFilename;

            // Convert and write out the song media file.
            ConversionQuality quality = _quality;

            ConversionFormat targetFormat;
            switch (context.TargetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsStoreApp:
                    targetFormat = ConversionFormat.WindowsMedia;
                    break;
                case TargetPlatform.DesktopGL:
                    targetFormat = ConversionFormat.Vorbis;
                    break;
                case TargetPlatform.BlazorGL:
                    targetFormat = ConversionFormat.Mp3;
                    break;
                case TargetPlatform.Android:
                    targetFormat = ConversionFormat.Aac;
                    break;
                case TargetPlatform.iOS:
                    targetFormat = ConversionFormat.Aac;
                    break;

                default:
                    targetFormat = ConversionFormat.Mp3;
                    break;
            }

            // Get the song output path with the target format extension.
            songFileName = Path.ChangeExtension(songFileName, SongProcessor.GetExtension(targetFormat));

            // Make sure the output folder for the file exists.
            Directory.CreateDirectory(Path.GetDirectoryName(songFileName));
            input.ConvertFormat(targetFormat, quality, songFileName);

            // Let the pipeline know about the song file so it can clean things up.
            context.AddOutputFile(songFileName);

            // Return the XNB song content.
            string relativeMediaPath = PathHelper.GetRelativePath(Path.GetDirectoryName(context.OutputFilename) + Path.DirectorySeparatorChar, songFileName);
            return new SongContent(relativeMediaPath, input.Duration);
        }

        /// <summary>
        /// Gets the file extension for an audio format.
        /// </summary>
        /// <param name="format">The conversion format</param>
        /// <returns>The file extension for the given conversion format.</returns>
        static public string GetExtension(ConversionFormat format)
        {
            switch (format)
            {
                case ConversionFormat.Adpcm:
                case ConversionFormat.Pcm:
                    return "wav";
                case ConversionFormat.WindowsMedia:
                    return "wma";
                case ConversionFormat.Xma:
                    return "xma";
                case ConversionFormat.ImaAdpcm:
                    return "wav";
                case ConversionFormat.Aac:
                    return "m4a";
                case ConversionFormat.Vorbis:
                    return "ogg";
                case ConversionFormat.Mp3:
                    return "mp3";

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
