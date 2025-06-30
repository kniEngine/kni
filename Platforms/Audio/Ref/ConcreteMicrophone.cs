// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.
    /// </summary>
    public sealed class ConcreteMicrophone : MicrophoneStrategy
    {

        public override TimeSpan BufferDuration
        {
            get { return base.BufferDuration; }
            set { base.BufferDuration = value; }
        }

        public override MicrophoneState State
        {
            get { return base.State; }
            set { base.State = value; }
        }


        internal ConcreteMicrophone()
            : base()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformStart(string deviceName)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformStop()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformIsHeadset()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformUpdate()
        {
            throw new PlatformNotSupportedException();
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
