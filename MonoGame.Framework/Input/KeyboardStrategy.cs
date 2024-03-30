// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class KeyboardStrategy
    {
        public abstract KeyboardState PlatformGetState();
    }
}