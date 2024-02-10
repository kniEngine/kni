// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;


namespace Microsoft.Xna.Framework.Content
{
    internal class SoundEffectReader : ContentTypeReader<SoundEffect>
    {
        protected override SoundEffect Read(ContentReader input, SoundEffect existingInstance)
        {         
            // XNB format for SoundEffect...
            //            
            // Byte [format size]	Format	WAVEFORMATEX structure
            // UInt32	Data size	
            // Byte [data size]	Data	Audio waveform data
            // Int32	Loop start	In bytes (start must be format block aligned)
            // Int32	Loop length	In bytes (length must be format block aligned)
            // Int32	Duration	In milliseconds

            // The header containss the WAVEFORMATEX header structure
            // defined as the following...
            //
            //  WORD  wFormatTag;       // byte[0]  +2
            //  WORD  nChannels;        // byte[2]  +2
            //  DWORD nSamplesPerSec;   // byte[4]  +4
            //  DWORD nAvgBytesPerSec;  // byte[8]  +4
            //  WORD  nBlockAlign;      // byte[12] +2
            //  WORD  wBitsPerSample;   // byte[14] +2
            //  WORD  cbSize;           // byte[16] +2
            //
            // We let the sound effect deal with parsing this based
            // on what format the audio data actually is.

            int headerSize = input.ReadInt32();
            byte[] header = input.ReadBytes(headerSize);

            // Read the audio data buffer.
            int dataSize = input.ReadInt32();
            byte[] data = input.BufferPool.Get(dataSize);
            input.Read(data, 0, dataSize);

            int loopStart = input.ReadInt32();
            int loopLength = input.ReadInt32();
            int durationMs = input.ReadInt32();

            // Create the effect.
            SoundEffect effect = existingInstance ?? new SoundEffect(header, data, dataSize, durationMs, loopStart, loopLength);

            // Store the original asset name for debugging later.
            effect.Name = input.AssetName;

            input.BufferPool.Return(data);

            return effect;
        }
    }
}
