// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using AudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        #region Initialization

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy)
            : base(audioServiceStrategy, sfxStrategy)
        {
            throw new PlatformNotSupportedException();

        }

        #endregion // Initialization

        public override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformPause()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformPlay(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformResume(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformRelease(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformUpdateState(ref SoundState state)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetPan(float pan)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetPitch(float pitch)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetVolume(float volume)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformClearFilter()
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
