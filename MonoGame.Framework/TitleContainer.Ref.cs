// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;


namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {

        private static Stream PlatformOpenStream(string safeName)
        {
            throw new PlatformNotSupportedException();
        }

    }
}

