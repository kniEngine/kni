// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.
    /// </summary>
    public sealed class ConcreteMicrophone : MicrophoneStrategy
    {

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
