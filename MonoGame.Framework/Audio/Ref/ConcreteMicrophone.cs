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

        internal override void PlatformStart(string deviceName)
        {
			throw new PlatformNotSupportedException();
        }

        internal override void PlatformStop()
        {
			throw new PlatformNotSupportedException();
        }

        internal override bool PlatformIsHeadset()
        {
            throw new PlatformNotSupportedException();
        }

        internal override bool PlatformUpdate()
		{
			throw new PlatformNotSupportedException();
		}
		
		internal override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
