// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.Utilities;
using nkast.Wasm.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    class ConcreteSoundEffect : SoundEffectStrategy
    {
        private AudioBuffer _audioBuffer;


        internal const int FormatPcm = 1;
        internal const int FormatMsAdpcm = 2;
        internal const int FormatIeee = 3;
        internal const int FormatIma4 = 17;

        #region Initialization

        public override void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            byte[] buffer;

            int audioFormat;
            int freq;
            int channels;
            int blockAlignment;
            int bitsPerSample;
            int samplesPerBlock;
            int sampleCount;

            buffer = AudioLoader.Load(stream, out audioFormat, out freq, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);

            duration = TimeSpan.FromSeconds((float)sampleCount / (float)freq);

            PlatformInitializeBuffer(buffer, 0, buffer.Length, audioFormat, channels, freq, blockAlignment, bitsPerSample, 0, sampleCount / channels);
        }

        private void PlatformInitializeBuffer(byte[] buffer, int bufferOffset, int bufferSize, int audioFormat, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            switch (audioFormat)
            {
                case AudioLoader.FormatPcm:
                    // PCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");
                    PlatformInitializePcm(buffer, bufferOffset, bufferSize, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    break;
                case AudioLoader.FormatIeee:
                    // IEEE Float
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    {
                        //InitializeIeeeFloat(buffer, bufferOffset, bufferSize, sampleRate, channels, loopStart, loopLength);
                        buffer = AudioLoader.ConvertFloatTo16(buffer, bufferOffset, bufferSize);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);

                    }
                    break;
                case AudioLoader.FormatMsAdpcm:
                    // Microsoft ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    {
                        // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, bufferOffset, bufferSize, channels, blockAlignment);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    break;
                case AudioLoader.FormatIma4:
                    // IMA4 ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    {
                        // If IMA/ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = AudioLoader.ConvertIma4ToPcm(buffer, bufferOffset, bufferSize, channels, blockAlignment);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    break;

                default:
                    throw new NotSupportedException("The specified sound format (" + audioFormat.ToString() + ") is not supported.");
            }
        }


        public override void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength)
        {
            short format = BitConverter.ToInt16(header, 0);
            short channels = BitConverter.ToInt16(header, 2);
            int sampleRate = BitConverter.ToInt32(header, 4);
            short blockAlignment = BitConverter.ToInt16(header, 12);
            short bitsPerSample = BitConverter.ToInt16(header, 14);

            switch (format)
            {
                case FormatPcm:
                    {
                        this.PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    }
                    break;

                case FormatMsAdpcm:
                    {
                        buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, 0, buffer.Length, channels, blockAlignment);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public override void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            if (index != 0)
                throw new NotImplementedException();
            if (loopStart != 0)
                throw new NotImplementedException();

            int numOfChannels = (int)channels;
            
            _audioBuffer = concreteAudioService.Context.CreateBuffer(numOfChannels, loopLength, sampleRate);

            // convert buffer to float (-1,+1) and set data for each channel.
            unsafe
            {
                fixed (void* pBuffer = buffer)
                {
                    switch (sampleBits)
                    {
                        case 16: // PCM 16bit
                            short* pBuffer16 = (short*)pBuffer;
                            float[] dest = new float[loopLength];
                            for (int c = 0; c < numOfChannels; c++)
                            {
                                for (int i = 0; i < loopLength; i++)
                                {
                                    dest[i] = (float)pBuffer16[i * numOfChannels] / (float)short.MaxValue;
                                }
                                _audioBuffer.CopyToChannel(dest, c);
                            }                           
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

        }

        public override void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

        }

        #endregion

        internal AudioBuffer GetAudioBuffer()
        {
            return _audioBuffer;
        }

#region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _audioBuffer.Dispose();
            }

            _audioBuffer = null;
        }

#endregion

    }
}

