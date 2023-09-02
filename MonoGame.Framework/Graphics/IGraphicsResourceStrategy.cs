// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IGraphicsResourceStrategy : IDisposable
    {
        GraphicsDevice GraphicsDevice { get; }

        event EventHandler<EventArgs> Disposing;
    }
}
