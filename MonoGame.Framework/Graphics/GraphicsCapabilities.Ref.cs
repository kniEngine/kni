// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {

        internal void PlatformInitialize(GraphicsDevice device)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
