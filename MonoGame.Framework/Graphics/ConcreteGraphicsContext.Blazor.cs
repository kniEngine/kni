// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {

        internal ConcreteGraphicsContext() : base()
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                base.Dispose();
            }
        }
    }
}
