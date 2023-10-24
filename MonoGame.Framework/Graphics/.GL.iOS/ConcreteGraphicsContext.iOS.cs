// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {

        }

        public override void BindDisposeContext()
        {
            Microsoft.Xna.Framework.Threading.EnsureUIThread();
        }

        public override void UnbindDisposeContext()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            base.Dispose(disposing);
        }

    }
}
