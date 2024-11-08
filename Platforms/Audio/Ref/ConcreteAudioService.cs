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
            }

        }

    }
}

