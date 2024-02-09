// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioService : AudioServiceStrategy
    {

        internal ConcreteAudioService()
        {
            throw new PlatformNotSupportedException();
        }

        public override SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy sfxStrategy, float pan)
        {
            return new ConcreteSoundEffectInstance(this, sfxStrategy, pan);
        }

        public override IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels, float pan)
        {
            return new ConcreteDynamicSoundEffectInstance(this, sampleRate, channels, pan);
        }

        public override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
            throw new PlatformNotSupportedException();
        }

        public override int PlatformGetMaxPlayingInstances()
        {
            return int.MaxValue;
        }

        public override void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}

