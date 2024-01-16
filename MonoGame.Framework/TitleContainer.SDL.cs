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

        private void PlatformInit()
        {
            // Check for the package Resources Folder first. This is where the assets
            // will be bundled.
            if (CurrentPlatform.OS == OS.MacOSX)
                _location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
            if (!Directory.Exists(Location))
                _location = AppDomain.CurrentDomain.BaseDirectory;
        }

        private Stream PlatformOpenStream(string safeName)
        {
            string absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
        }
    }
}

