// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private void PlatformGraphicsDeviceResetting()
        {
            throw new PlatformNotSupportedException();
        }
    }
}
