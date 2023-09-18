// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsCapabilities : GraphicsCapabilities
    {

        internal void PlatformInitialize(GraphicsDeviceStrategy deviceStrategy)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
