// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {

        internal void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            throw new PlatformNotSupportedException();
        }

    }
}

