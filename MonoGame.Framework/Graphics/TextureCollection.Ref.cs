// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {

        void PlatformInit(int capacity)
        {
            throw new PlatformNotSupportedException();
        }

        void PlatformClear()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApply()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
