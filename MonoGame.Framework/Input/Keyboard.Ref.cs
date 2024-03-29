// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Keyboard
    {
        private KeyboardState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
