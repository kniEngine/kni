// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        public override string Location { get { throw new PlatformNotSupportedException(); } }

        public ConcreteTitleContainer() : base()
        {
            throw new PlatformNotSupportedException();
        }

        public override Stream PlatformOpenStream(string name)
        {
            throw new PlatformNotSupportedException();
        }
    }
}

