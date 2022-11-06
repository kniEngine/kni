// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsContextGL : GraphicsContextStrategy
    {

        internal ConcreteGraphicsContextGL(GraphicsDevice device)
            : base(device)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                base.Dispose(disposing);
            }
        }

    }
}
