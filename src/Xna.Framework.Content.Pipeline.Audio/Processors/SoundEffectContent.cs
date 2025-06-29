// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Represents a processed sound effect.
    /// </summary>
    public sealed class SoundEffectContent
    {
        internal byte[] _format;
        internal byte[] _data;
        internal int _loopStart;
        internal int _loopLength;
        internal int _duration;

        /// <summary>
        /// Initializes a new instance of the SoundEffectContent class.
        /// </summary>
        /// <param name="format">The WAV header.</param>
        /// <param name="data">The audio waveform data.</param>
        /// <param name="loopStart">The start of the loop segment (must be block aligned).</param>
        /// <param name="loopLength">The length of the loop segment (must be block aligned).</param>
        /// <param name="duration">The duration of the wave file in milliseconds.</param>
        internal SoundEffectContent(byte[] format, byte[] data, int loopStart, int loopLength, int duration)
        {
            this._format = format;
            this._data = data;
            this._loopStart = loopStart;
            this._loopLength = loopLength;
            this._duration = duration;
        }
    }
}
