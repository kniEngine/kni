// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Audio.OpenAL;

namespace Microsoft.Xna.Platform.Audio
{
    sealed internal class ALSoundBuffer : IDisposable
    {
        internal AudioService _audioService;

        internal int _bufferId;
        bool _isBufferDisposed;


        public ALSoundBuffer(AudioService audioService)
        {
            _audioService = audioService;
            _audioService.Disposing += _audioService_Disposing;
        }

        ~ALSoundBuffer()
        {
            Dispose(false);
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

        private void Dispose(bool disposing)
        {
            if (!_isBufferDisposed)
            {
                ConcreteAudioService concreteAudioService = ((IPlatformAudioService)_audioService).Strategy.ToConcrete<ConcreteAudioService>();

                concreteAudioService.OpenAL.DeleteBuffer(_bufferId);
                concreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
                _isBufferDisposed = true;
                _bufferId = 0;

                _audioService.Disposing -= _audioService_Disposing;
                _audioService = null;
            }
        }
    }
}
