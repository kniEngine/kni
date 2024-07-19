// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using nkast.Wasm.Input;

namespace Microsoft.Xna.Platform.Input
{
    internal class BlazorGamePadDevice : GamePadDevice
    {
        public int _index;

        public BlazorGamePadDevice(int index)
            : base()
        {
            _index = index;
        }
    }

}
