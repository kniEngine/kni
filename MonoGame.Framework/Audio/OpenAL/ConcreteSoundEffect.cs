// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;
using Microsoft.Xna.Platform.Audio.Utilities;

#if IOS || TVOS
using AudioToolbox;
using AudioUnit;
#endif

namespace Microsoft.Xna.Platform.Audio
{
    class ConcreteSoundEffect : SoundEffectStrategy
    {
        private ALSoundBuffer _soundBuffer;


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

            PlatformInitializeBuffer(buffer, 0, buffer.Length, audioFormat, channels, freq, blockAlignment, bitsPerSample, 0, 0);
        }

        public override void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength)
        {
            short audioFormat = BitConverter.ToInt16(header, 0);
            short channels = BitConverter.ToInt16(header, 2);
            int sampleRate = BitConverter.ToInt32(header, 4);
            short blockAlignment = BitConverter.ToInt16(header, 12);
            short bitsPerSample = BitConverter.ToInt16(header, 14);

            PlatformInitializeBuffer(buffer, index, count, audioFormat, channels, sampleRate, blockAlignment, bitsPerSample, loopStart, loopLength);
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
                case AudioLoader.FormatMsAdpcm:
                    // Microsoft ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");
                    if (!concreteAudioService.SupportsAdpcm)
                    {
                        // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, bufferOffset, bufferSize, channels, blockAlignment);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeAdpcm(buffer, bufferOffset, bufferSize, sampleRate, channels, blockAlignment, loopStart, loopLength);
                    }
                    break;
                case AudioLoader.FormatIeee:
                    // IEEE Float
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");
                    if (!concreteAudioService.SupportsIeee)
                    {
                        // If 32-bit IEEE float is not supported, convert to 16-bit signed PCM
                        buffer = AudioLoader.ConvertFloatTo16(buffer, bufferOffset, bufferSize);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeIeeeFloat(buffer, bufferOffset, bufferSize, sampleRate, channels, loopStart, loopLength);
                    }
                    break;
                case AudioLoader.FormatIma4:
                    // IMA4 ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");
                    if (!concreteAudioService.SupportsIma4)
                    {
                        // If IMA/ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = AudioLoader.ConvertIma4ToPcm(buffer, bufferOffset, bufferSize, channels, blockAlignment);
                        PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeIma4(buffer, bufferOffset, bufferSize, sampleRate, channels, blockAlignment, loopStart, loopLength);
                    }
                    break;

                default:
                    throw new NotSupportedException("The specified sound format (" + audioFormat.ToString() + ") is not supported.");
            }
        }

        public override void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength)
        {
            if (sampleBits == 24)
            {
                // Convert 24-bit signed PCM to 16-bit signed PCM
                buffer = AudioLoader.Convert24To16(buffer, index, count);
                index = 0;
                count = buffer.Length;
                sampleBits = 16;
            }

            ALFormat alFormat = GetSoundFormat(AudioLoader.FormatPcm, channels, sampleBits);

            // bind buffer
            _soundBuffer = new ALSoundBuffer(AudioService.Current);
            _soundBuffer.BindDataBuffer(buffer, index, count, alFormat, sampleRate);
        }

        public override void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            if (!concreteAudioService.SupportsAdpcm)
            {
                // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, index, count, channels, blockAlignment);
                PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
            }
            else
            {
                InitializeAdpcm(buffer, index, count, sampleRate, channels, (blockAlignment + 22) * channels, loopStart, loopLength);
            }
        }

        private void InitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, int channels, int loopStart, int loopLength)
        {
            ALFormat alFormat = GetSoundFormat(AudioLoader.FormatIeee, channels, 32);

            // bind buffer
            _soundBuffer = new ALSoundBuffer(AudioService.Current);
            _soundBuffer.BindDataBuffer(buffer, offset, count, alFormat, sampleRate);
        }

        private void InitializeAdpcm(byte[] buffer, int index, int count, int sampleRate, int channels, int blockAlignment, int loopStart, int loopLength)
        {           

            ALFormat alFormat = GetSoundFormat(AudioLoader.FormatMsAdpcm, channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(AudioLoader.FormatMsAdpcm, channels, 0, blockAlignment);

            // Buffer length must be aligned with the block alignment
            int alignedCount = count - (count % blockAlignment);

            // bind buffer
            _soundBuffer = new ALSoundBuffer(AudioService.Current);
            _soundBuffer.BindDataBuffer(buffer, index, alignedCount, alFormat, sampleRate, sampleAlignment);
        }

        private void InitializeIma4(byte[] buffer, int index, int count, int sampleRate, int channels, int blockAlignment, int loopStart, int loopLength)
        {
            ALFormat alFormat = GetSoundFormat(AudioLoader.FormatIma4, (int)channels, 0);
            int sampleAlignment = AudioLoader.SampleAlignment(AudioLoader.FormatIma4, channels, 0, blockAlignment);

            // bind buffer
            _soundBuffer = new ALSoundBuffer(AudioService.Current);
            _soundBuffer.BindDataBuffer(buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }

        #endregion

        internal ALSoundBuffer GetALSoundBuffer()
        {
            return _soundBuffer;
        }


        public static ALFormat GetSoundFormat(int audioFormat, int channels, int bitsPerSample)
        {
            switch (audioFormat)
            {
                case AudioLoader.FormatPcm: 
                    // PCM
                    switch (channels)
                    {
                        case 1: return bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                        case 2: return bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case AudioLoader.FormatMsAdpcm:
                    // Microsoft ADPCM
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoMSAdpcm;
                        case 2: return ALFormat.StereoMSAdpcm;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case AudioLoader.FormatIeee:
                    // IEEE Float
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoFloat32;
                        case 2: return ALFormat.StereoFloat32;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                case AudioLoader.FormatIma4:
                    // IMA4 ADPCM
                    switch (channels)
                    {
                        case 1: return ALFormat.MonoIma4;
                        case 2: return ALFormat.StereoIma4;
                        default: throw new NotSupportedException("The specified channel count is not supported.");
                    }
                default:
                    throw new NotSupportedException("The specified sound format (" + audioFormat.ToString() + ") is not supported.");
            }
        }


        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _soundBuffer.Dispose();
                _soundBuffer = null;
            }

        }

#endregion

    }
}

