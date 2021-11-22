// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ALSoundBuffer : IDisposable
    {
        internal AudioService _audioService { get; private set; }
        bool _isDisposed;

        public int Buffer
        {
            get;
            private set;
        }


        public ALSoundBuffer(AudioService audioService)
        {
            if (audioService == null)
                throw new ArgumentNullException("audioService");

            _audioService = audioService;
            _audioService.Disposing += _audioService_Disposing;

            Buffer = AL.GenBuffer();
            ALHelper.CheckError("Failed to generate OpenAL data buffer.");
        }

        ~ALSoundBuffer()
        {
            Dispose(false);
        }
        

        public void BindDataBuffer(byte[] dataBuffer, int index, int count, ALFormat format, int sampleRate, int sampleAlignment = 0)
        {
            ConcreteAudioService ConcreteAudioService = (ConcreteAudioService)AudioService.Current._strategy;

            if ((format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm) && !ConcreteAudioService.SupportsAdpcm)
                throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
            if ((format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4) && !ConcreteAudioService.SupportsIma4)
                throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");
            
            AL.BufferData(Buffer, format, dataBuffer, index, count, sampleRate, sampleAlignment);
            ALHelper.CheckError("Failed to fill buffer.");
        }

        private void _audioService_Disposing(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // Clean up managed objects
            }

            // Release unmanaged resources
            AL.DeleteBuffer(Buffer);
            ALHelper.CheckError("Failed to delete buffer.");
            Buffer = 0;

            _audioService.Disposing -= _audioService_Disposing;
            _audioService = null;

            _isDisposed = true;
        }
    }
}
