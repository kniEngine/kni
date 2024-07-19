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

        public int Buffer { get; private set; }

        internal ConcreteAudioService ConcreteAudioService { get { return ((IPlatformAudioService)_audioService).Strategy.ToConcrete<ConcreteAudioService>(); } }


        public ALSoundBuffer(AudioService audioService)
        {
            if (audioService == null)
                throw new ArgumentNullException("audioService");

            _audioService = audioService;
            _audioService.Disposing += _audioService_Disposing;

            Buffer = ConcreteAudioService.OpenAL.GenBuffer();
            ConcreteAudioService.OpenAL.CheckError("Failed to generate OpenAL data buffer.");
        }

        ~ALSoundBuffer()
        {
            Dispose(false);
        }
        

        public void BindDataBuffer(byte[] dataBuffer, int index, int count, ALFormat alFormat, int sampleRate, int sampleAlignment = 0)
        {
            if ((alFormat == ALFormat.MonoMSAdpcm || alFormat == ALFormat.StereoMSAdpcm) && !ConcreteAudioService.SupportsAdpcm)
                throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
            if ((alFormat == ALFormat.MonoIma4 || alFormat == ALFormat.StereoIma4) && !ConcreteAudioService.SupportsIma4)
                throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");

            ConcreteAudioService.OpenAL.BufferData(Buffer, alFormat, dataBuffer, index, count, sampleRate, sampleAlignment);
            ConcreteAudioService.OpenAL.CheckError("Failed to fill buffer.");
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
            ConcreteAudioService.OpenAL.DeleteBuffer(Buffer);
            ConcreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
            Buffer = 0;

            _audioService.Disposing -= _audioService_Disposing;
            _audioService = null;

            _isDisposed = true;
        }
    }
}
