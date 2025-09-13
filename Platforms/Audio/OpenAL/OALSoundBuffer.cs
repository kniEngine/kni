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
        internal AudioService _audioService;
        bool _isDisposed;

        internal int _bufferId;

        public ALSoundBuffer(AudioService audioService)
        {
            if (audioService == null)
                throw new ArgumentNullException("audioService");

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

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

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

            _isDisposed = true;
        }
    }
}
