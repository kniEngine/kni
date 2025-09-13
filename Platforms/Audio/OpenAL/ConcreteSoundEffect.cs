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

        private void PlatformInitializeBuffer(byte[] buffer, int index, int count, int audioFormat, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            switch (audioFormat)
            {
                case AudioLoader.FormatPcm:
                    // PCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    break;

                case AudioLoader.FormatIeee:
                    // IEEE Float
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    if (!concreteAudioService.SupportsIeee)
                    {
                        // If 32-bit IEEE float is not supported, convert to 16-bit signed PCM
                        buffer = AudioLoader.ConvertFloatTo16(buffer, index, count);
                        index = 0;
                        count = buffer.Length;
                        bitsPerSample = 16;
                        PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeIeeeFloat(concreteAudioService, buffer, index, count, sampleRate, channels, loopStart, loopLength);
                    }
                    break;

                case AudioLoader.FormatMsAdpcm:
                    // Microsoft ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    if (!concreteAudioService.SupportsAdpcm)
                    {
                        // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, index, count, channels, blockAlignment);
                        index = 0;
                        count = buffer.Length;
                        bitsPerSample = 16;
                        PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeAdpcm(concreteAudioService, buffer, index, count, sampleRate, channels, blockAlignment, loopStart, loopLength);
                    }
                    break;
                case AudioLoader.FormatIma4:
                    // IMA4 ADPCM
                    if (channels < 1 || 2 < channels)
                        throw new NotSupportedException("The specified channel count (" + channels + ") is not supported.");

                    if (!concreteAudioService.SupportsIma4)
                    {
                        // If IMA/ADPCM is not supported, convert to 16-bit signed PCM
                        buffer = AudioLoader.ConvertIma4ToPcm(buffer, index, count, channels, blockAlignment);
                        index = 0;
                        count = buffer.Length;
                        bitsPerSample = 16;
                        PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                    }
                    else
                    {
                        InitializeIma4(concreteAudioService, buffer, index, count, sampleRate, channels, blockAlignment, loopStart, loopLength);
                    }
                    break;

                default:
                    throw new NotSupportedException("The specified sound format (" + audioFormat.ToString() + ") is not supported.");
            }
        }

        public override void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            if (sampleBits == 24)
            {
                // Convert 24-bit signed PCM to 16-bit signed PCM
                buffer = AudioLoader.Convert24To16(buffer, index, count);
                index = 0;
                count = buffer.Length;
                sampleBits = 16;
            }

            int sampleAlignment = 0;
            ALFormat alFormat;
            switch (channels)
            {
                case 1: alFormat = (sampleBits == 8) ? ALFormat.Mono8 : ALFormat.Mono16; break;
                case 2: alFormat = (sampleBits == 8) ? ALFormat.Stereo8 : ALFormat.Stereo16; break;
                default: throw new NotSupportedException("The specified channel count is not supported.");
            }

            CreateBuffer(concreteAudioService, buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }

        public override void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            ConcreteAudioService concreteAudioService = ((IPlatformAudioService)AudioService.Current).Strategy.ToConcrete<ConcreteAudioService>();

            if (!concreteAudioService.SupportsAdpcm)
            {
                // If MS-ADPCM is not supported, convert to 16-bit signed PCM
                buffer = MsAdpcmDecoder.ConvertMsAdpcmToPcm(buffer, index, count, channels, blockAlignment);
                index = 0;
                count = buffer.Length;
                int sampleBits = 16;
                PlatformInitializePcm(buffer, index, count, sampleBits, sampleRate, channels, loopStart, loopLength);
            }
            else
            {
                InitializeAdpcm(concreteAudioService, buffer, index, count, sampleRate, channels, (blockAlignment + 22) * channels, loopStart, loopLength);
            }
        }

        private void InitializeIeeeFloat(ConcreteAudioService concreteAudioService, byte[] buffer, int index, int count, int sampleRate, int channels, int loopStart, int loopLength)
        {
            int sampleAlignment = 0;
            ALFormat alFormat;
            switch (channels)
            {
                case 1: alFormat = ALFormat.MonoFloat32; break;
                case 2: alFormat = ALFormat.StereoFloat32; break;
                default: throw new NotSupportedException("The specified channel count is not supported.");
            }

            CreateBuffer(concreteAudioService, buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }

        private void InitializeAdpcm(ConcreteAudioService concreteAudioService, byte[] buffer, int index, int count, int sampleRate, int channels, int blockAlignment, int loopStart, int loopLength)
        {           
            int sampleAlignment = AudioLoader.SampleAlignment(AudioLoader.FormatMsAdpcm, channels, 0, blockAlignment);
            // Buffer length must be aligned with the block alignment
            count = count - (count % blockAlignment);
            ALFormat alFormat;
            switch (channels)
            {
                case 1: alFormat = ALFormat.MonoMSAdpcm; break;
                case 2: alFormat = ALFormat.StereoMSAdpcm; break;
                default: throw new NotSupportedException("The specified channel count is not supported.");
            }

            CreateBuffer(concreteAudioService, buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }

        private void InitializeIma4(ConcreteAudioService concreteAudioService, byte[] buffer, int index, int count, int sampleRate, int channels, int blockAlignment, int loopStart, int loopLength)
        {
            int sampleAlignment = AudioLoader.SampleAlignment(AudioLoader.FormatIma4, channels, 0, blockAlignment);
            ALFormat alFormat;
            switch (channels)
            {
                case 1: alFormat = ALFormat.MonoIma4; break;
                case 2: alFormat = ALFormat.StereoIma4; break;
                default: throw new NotSupportedException("The specified channel count is not supported.");
            }

            CreateBuffer(concreteAudioService, buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }

        private void CreateBuffer(ConcreteAudioService concreteAudioService, byte[] buffer, int index, int count, ALFormat alFormat, int sampleRate, int sampleAlignment)
        {
            _soundBuffer = new ALSoundBuffer(AudioService.Current);
            ALSoundBuffer.BindDataBuffer(concreteAudioService, _soundBuffer._bufferId, buffer, index, count, alFormat, sampleRate, sampleAlignment);
        }


        #endregion

        internal int GetALBufferId()
        {
            return _soundBuffer._bufferId;
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

