// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using AudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        public override bool IsXAct
        {
            get { return base.IsXAct; }
            set { base.IsXAct = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Pan
        {
            get { return base.Pan; }
            set { base.Pan = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;

                // XAct sound effects are not tied to the SoundEffect master volume.
                if (this.IsXAct)
                    this.PlatformSetVolume(value);
                else
                    this.PlatformSetVolume(value * SoundEffect.MasterVolume);
            }
        }

        public override float Pitch
        {
            get { return base.Pitch; }
            set
            {
                base.Pitch = value;

                this.PlatformSetPitch(base.Pitch);
            }
        }

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
