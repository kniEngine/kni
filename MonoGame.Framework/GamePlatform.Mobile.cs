// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Platform
{
    partial class GameStrategy
    {
        internal static GameStrategy PlatformCreate(Game game)
        {
            return new ConcreteGame(game);
        }
    }
}
