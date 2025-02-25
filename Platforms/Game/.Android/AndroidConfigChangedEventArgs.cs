// Copyright (C)2025 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Platform
{
    internal class AndroidConfigChangedEventArgs : EventArgs
    {
        public readonly Android.Content.Res.Configuration NewConfig;

        public AndroidConfigChangedEventArgs(Android.Content.Res.Configuration newConfig)
        {
            NewConfig = newConfig;
        }
    }

    internal class AndroidConfigChangedOrientationEventArgs : AndroidConfigChangedEventArgs
    {
        public readonly Android.Content.Res.Orientation NewOrientation;

        public AndroidConfigChangedOrientationEventArgs(Android.Content.Res.Configuration newConfig, Android.Content.Res.Orientation newOrientation)
              : base(newConfig)
        {
            this.NewOrientation = newOrientation;
        }
    }

}
