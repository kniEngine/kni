﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A sound effect processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a SoundEffect type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Sound Effect - KNI")]
    public class SoundEffectProcessor : ContentProcessor<AudioContent, SoundEffectContent>
    {
        ConversionQuality quality = ConversionQuality.Best;

        /// <summary>
        /// Gets or sets the target format quality of the audio content.
        /// </summary>
        /// <value>The ConversionQuality of this audio data.</value>
        public ConversionQuality Quality { get { return quality; } set { quality = value; } }

        /// <summary>
        /// Initializes a new instance of SoundEffectProcessor.
        /// </summary>
        public SoundEffectProcessor()
        {
        }

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SoundEffectContent Process(AudioContent input, ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (context == null)
                throw new ArgumentNullException("context");

            var profile = AudioProfile.ForPlatform(context.TargetPlatform);
            var finalQuality = profile.ConvertAudio(context.TargetPlatform, quality, input);
            if (quality != finalQuality)
                context.Logger.LogMessage("Failed to convert using \"{0}\" quality, used \"{1}\" quality", quality, finalQuality);

            return new SoundEffectContent(input.Format.NativeWaveFormat, input.Data, input.LoopStart, input.LoopLength, (int)input.Duration.TotalMilliseconds);
        }
    }
}
