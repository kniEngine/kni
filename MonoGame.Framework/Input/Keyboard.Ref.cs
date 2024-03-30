// Copyright (C)2022-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboard : KeyboardStrategy
    {

        public override KeyboardState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
