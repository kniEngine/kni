// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {

        private void PlatformInitialize(GraphicsDevice device)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
