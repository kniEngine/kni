// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioService : AudioServiceStrategy
    {
        internal AudioContext Context { get; private set; }


        internal ConcreteAudioService()
        {
            Context = new AudioContext();
        }

        public override SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy sfxStrategy)
        {
            return new ConcreteSoundEffectInstance(this, sfxStrategy);
        }

        public override IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels)
        {
            return new ConcreteDynamicSoundEffectInstance(this, sampleRate, channels);
        }

        public override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
        }

        public override int PlatformGetMaxPlayingInstances()
        {
            // These platforms are only limited by memory.
            return int.MaxValue;
        }

        public override void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
        }

        public override void Suspend()
        {
        }

        public override void Resume()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                Context.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects)
            // TODO: set large fields to null.

            Context = null;
        }

    }
}

