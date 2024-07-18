// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    class ConcreteSoundEffect : SoundEffectStrategy
    {
        private AudioBuffer _audioBuffer;


        #region Initialization

        public override void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;

            throw new NotImplementedException();
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
                case 1:
                    {
                        this.PlatformInitializePcm(buffer, index, count, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                        return;
                    }
                    break;
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

