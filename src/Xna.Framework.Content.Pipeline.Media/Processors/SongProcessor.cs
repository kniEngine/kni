// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
            AudioProfile profile = AudioProfile.ForPlatform(context.TargetPlatform);
            ConversionQuality finalQuality = profile.ConvertStreamingAudio(context.TargetPlatform, _quality, input, ref songFileName);

            // Let the pipeline know about the song file so it can clean things up.
            context.AddOutputFile(songFileName);
            if (_quality != finalQuality)
                context.Logger.LogMessage("Failed to convert using \"{0}\" quality, used \"{1}\" quality", _quality, finalQuality);

            // Return the XNB song content.
            string relativeMediaPath = PathHelper.GetRelativePath(Path.GetDirectoryName(context.OutputFilename) + Path.DirectorySeparatorChar, songFileName);
            return new SongContent(relativeMediaPath, input.Duration);
        }
    }
}
