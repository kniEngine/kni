// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;

#if WINDOWS_UAP
using Windows.UI.ViewManagement;
#endif


namespace Microsoft.Xna.Platform
{
    partial class GameStrategy
    {
        internal static GameStrategy PlatformCreate(Game game)
        {
#if DESKTOPGL
            return new SdlGamePlatform(game);
#elif WINDOWS
            return new WinFormsGamePlatform(game);
#elif WINDOWS_UAP
            return new UAPGamePlatform(game);
#endif
        }
   }
}
