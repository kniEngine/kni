// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;
using Microsoft.Xna.Platform.Utilities;

using Microsoft.Xna.Framework;
using System.Globalization;
using Android.Content.PM;
using Android.Content;
using Android.Media;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioServiceDroid: ConcreteAudioService
    {
    
        internal ConcreteAudioServiceDroid() : base()
        {
        }

        public override int PlatformGetMaxPlayingInstances()
        {
            return 32;
        }

        public override void Suspend()
        {
            // Pause all currently playing sounds by pausing the mixer
            OpenAL.ALC.DevicePause(base.ALDevice);
        }

        public override void Resume()
        {
            // Resume all sounds that were playing when the activity was paused
            OpenAL.ALC.DeviceResume(base.ALDevice);
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

