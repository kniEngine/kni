// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework
{
    partial class GamePlatform
    {

        internal static GamePlatform PlatformCreate(Game game)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
