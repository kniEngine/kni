// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioService : AudioServiceStrategy
    {


        internal ConcreteAudioService()
        {

        }

        internal override SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy sfxStrategy, float pan)
        {
            return new ConcreteSoundEffectInstance(this, sfxStrategy, pan);
        }

        internal override IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, AudioChannels channels, float pan)
        {
            return new ConcreteDynamicSoundEffectInstance(this, sampleRate, channels, pan);
        }

        internal override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
        }

        internal override int PlatformGetMaxPlayingInstances()
        {
            // These platforms are only limited by memory.
            return int.MaxValue;
        }        

        internal override void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects)
            // TODO: set large fields to null.

        }
    }
}

