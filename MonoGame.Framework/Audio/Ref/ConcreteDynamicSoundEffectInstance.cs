﻿// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        public event EventHandler<EventArgs> OnBufferNeeded;

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels, float pan)
            : base(audioServiceStrategy, null, pan)
        {
            throw new PlatformNotSupportedException();
        }

        public int DynamicPlatformGetPendingBufferCount()
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
