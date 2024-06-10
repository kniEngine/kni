// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Windows.ApplicationModel;
using WGI = Windows.Gaming.Input;

namespace Microsoft.Xna.Platform.Input
{
    internal class WGIGamePadDevice : GamePadDevice
    {
        public WGI.Gamepad _device;

        public WGIGamePadDevice(WGI.Gamepad device)
            : base()
        {
            _device = device;

        }
    }
}
