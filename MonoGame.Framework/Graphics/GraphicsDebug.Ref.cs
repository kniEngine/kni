// Copyright (C)2022 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDebug
    {

        private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
