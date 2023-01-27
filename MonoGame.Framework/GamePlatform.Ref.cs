// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Platform
{
    partial class GameStrategy
    {

        internal static GameStrategy PlatformCreate(Game game)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
