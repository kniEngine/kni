// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        private readonly WeakReference _dynamicSoundEffectInstanceRef = new WeakReference(null);
        DynamicSoundEffectInstance IDynamicSoundEffectInstanceStrategy.DynamicSoundEffectInstance
        {
            get { return _dynamicSoundEffectInstanceRef.Target as DynamicSoundEffectInstance; }
            set { _dynamicSoundEffectInstanceRef.Target = value; }
        }

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels)
            : base(audioServiceStrategy, null)
        {
            throw new PlatformNotSupportedException();
        }

        public int BuffersNeeded { get; set; }

        public int DynamicPlatformGetPendingBufferCount()
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
            System.Diagnostics.Debug.Assert(isLooped == false);

            throw new PlatformNotSupportedException();
        }

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            throw new PlatformNotSupportedException();
        }

        public void DynamicPlatformClearBuffers()
        {
            throw new PlatformNotSupportedException();
        }

        public void DynamicPlatformUpdateBuffers()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
