// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {
        private static KeyboardState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
