// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        private string _location = string.Empty;

        private TitlePlatform _platform = TitlePlatform.SDL;

        public override string Location { get { return _location; } }

        public override TitlePlatform Platform { get { return _platform; } }

        public ConcreteTitleContainer() : base()
        {
            // Check for the package Resources Folder first. This is where the assets
            // will be bundled.
            if (CurrentPlatform.OS == OS.MacOSX)
                _location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
            if (!Directory.Exists(_location))
                _location = AppDomain.CurrentDomain.BaseDirectory;
        }

        public override Stream PlatformOpenStream(string name)
        {
            string absolutePath = Path.Combine(_location, name);

            return File.OpenRead(absolutePath);
        }
    }
}

