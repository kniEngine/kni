// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContext(GraphicsDevice device)
            : base(device)
        {

        }

        public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApplyViewport()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformResolveRenderTargets()
        {
            throw new PlatformNotSupportedException();
        }

        internal void PlatformApplyDefaultRenderTarget()
        {
            throw new PlatformNotSupportedException();
        }

        internal IRenderTarget PlatformApplyRenderTargets()
        {
            throw new PlatformNotSupportedException();
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
