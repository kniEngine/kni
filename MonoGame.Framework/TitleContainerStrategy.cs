// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{

    public abstract class TitleContainerStrategy
    {
        public abstract string Location { get; }

        protected TitleContainerStrategy()
        {
            
        }

        public abstract Stream PlatformOpenStream(string name);

    }
}
