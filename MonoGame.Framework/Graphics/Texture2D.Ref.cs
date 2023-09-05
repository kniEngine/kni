// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        private void PlatformConstructTexture2D(int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            throw new PlatformNotSupportedException();
        }

    }
}

