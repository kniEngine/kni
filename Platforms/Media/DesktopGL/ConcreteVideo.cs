// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoStrategy : VideoStrategy
    {

        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }
    }
}
