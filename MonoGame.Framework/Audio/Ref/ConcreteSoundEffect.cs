// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    class ConcreteSoundEffect : SoundEffectStrategy
    {

        #region Initialization

        internal override void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;

            throw new PlatformNotSupportedException();
        }

        internal override void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            throw new PlatformNotSupportedException();
        }

        #endregion


#region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

#endregion

    }
}

