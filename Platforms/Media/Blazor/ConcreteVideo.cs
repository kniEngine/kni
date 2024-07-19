// Copyright (C)2023 Nick Kastellanos

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
