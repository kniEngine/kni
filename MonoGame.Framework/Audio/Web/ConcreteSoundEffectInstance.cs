// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        private AudioServiceStrategy _audioServiceStrategy;
        private ConcreteSoundEffect _concreteSoundEffect;
        internal ConcreteAudioService ConcreteAudioService { get { return (ConcreteAudioService)_audioServiceStrategy; } }


        #region Initialization

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy, float pan)
            : base(audioServiceStrategy, sfxStrategy, pan)
        {
            _audioServiceStrategy = audioServiceStrategy;
            _concreteSoundEffect = (ConcreteSoundEffect)sfxStrategy;

        }

        #endregion // Initialization

        internal override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
        }

        internal override void PlatformPause()
        {
        }

        internal override void PlatformPlay(bool isLooped)
        {
        }

        internal override void PlatformResume(bool isLooped)
        {
        }

        internal override void PlatformStop()
        {
        }

        internal override void PlatformRelease(bool isLooped)
        {
        }

        internal override bool PlatformUpdateState(ref SoundState state)
        {
            return false;
        }

        internal override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
        }

        internal override void PlatformSetPan(float pan)
        {
        }

        internal override void PlatformSetPitch(float pitch)
        {
        }

        internal override void PlatformSetVolume(float volume)
        {
        }

        internal override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
        }

        internal override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
        }

        internal override void PlatformClearFilter()
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
