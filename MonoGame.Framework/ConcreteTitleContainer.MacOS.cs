// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        private string _location = string.Empty;

        public override string Location { get { return _location; } }

        public ConcreteTitleContainer() : base()
        {
            _location = NSBundle.MainBundle.ResourcePath;
        }

        public override Stream PlatformOpenStream(string name)
        {
            string absolutePath = Path.Combine(_location, name);

            return File.OpenRead(absolutePath);
        }
    }
}

