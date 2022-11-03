// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsContextGL : GraphicsContextStrategy
    {

        protected ConcreteGraphicsContextGL() : base()
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
