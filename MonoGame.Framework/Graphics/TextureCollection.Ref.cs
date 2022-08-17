// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {

        void PlatformInit()
        {
            throw new PlatformNotSupportedException();
        }

        void PlatformClear()
        {
            throw new PlatformNotSupportedException();
        }

        void PlatformSetTextures(GraphicsDevice device)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
