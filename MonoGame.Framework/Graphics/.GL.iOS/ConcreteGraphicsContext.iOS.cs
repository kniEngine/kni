// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private int _glContextCurrentThreadId = -1;

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override void BindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            throw new InvalidOperationException("Operation not called on UI thread.");
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
