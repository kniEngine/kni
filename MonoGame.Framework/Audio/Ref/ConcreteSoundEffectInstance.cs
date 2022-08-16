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

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy, float pan)
            : base(audioServiceStrategy, sfxStrategy, pan)
        {
            throw new PlatformNotSupportedException();

        }

        #endregion // Initialization

        internal override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformPause()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformPlay(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformResume(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformRelease(bool isLooped)
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformUpdateState(ref SoundState state)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetPan(float pan)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetPitch(float pitch)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetVolume(float volume)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
            throw new PlatformNotSupportedException();
        }

        internal override void PlatformClearFilter()
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
