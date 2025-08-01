﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
        }

        public override void PlatformStart(string deviceName)
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            throw new NotImplementedException();
        }

        public override bool PlatformIsHeadset()
        {
            throw new NotImplementedException();
        }

        public override bool PlatformUpdate()
        {
            throw new NotImplementedException();
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
