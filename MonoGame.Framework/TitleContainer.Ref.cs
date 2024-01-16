// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;


namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {

        private void PlatformInit()
        {
            throw new PlatformNotSupportedException();
        }

        private Stream PlatformOpenStream(string safeName)
        {
            throw new PlatformNotSupportedException();
        }

    }
}

