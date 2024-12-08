// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Devices;
using nkast.Wasm.Dom;

namespace Microsoft.Xna.Platform.Devices
{
    public sealed class ConcreteVibrator : VibratorStrategy
    {

        public ConcreteVibrator()
        {
        }

        public override void Vibrate(TimeSpan duration)
        {
            Window.Current.Navigator.Vibrate(duration);
        }
    }
}
