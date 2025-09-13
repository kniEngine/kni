// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;

namespace Microsoft.Xna.Platform.Audio
{
    sealed internal class ALSoundBuffer : IDisposable
    {
        internal AudioService _audioService;

        internal int _bufferId;
        private bool _isBufferDisposed;

        public ALSoundBuffer(AudioService audioService)
        {
            _audioService = audioService;
        }

        ~ALSoundBuffer()
        {
            Dispose(false);
        }

        internal void _audioService_Disposing(object sender, EventArgs e)
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
                if (disposing)
                {
                    // Clean up managed objects
                }

                ConcreteAudioService concreteAudioService = ((IPlatformAudioService)_audioService).Strategy.ToConcrete<ConcreteAudioService>();

                // Release unmanaged resources
                concreteAudioService.OpenAL.DeleteBuffer(_bufferId);
                concreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
                _bufferId = 0;

                _audioService.Disposing -= _audioService_Disposing;
                _audioService = null;

                _isBufferDisposed = true;
            }
        }
    }
}
