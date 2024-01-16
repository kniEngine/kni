// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        static partial void PlatformInit()
        {
            Location = AppDomain.CurrentDomain.BaseDirectory;
        }

        private static Stream PlatformOpenStream(string safeName)
        {
            string absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
        }
    }
}

