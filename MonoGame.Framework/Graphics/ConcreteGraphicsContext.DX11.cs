// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {
        private D3D11.DeviceContext _d3dContext;

        internal D3D11.DeviceContext D3dContext { get { return _d3dContext; } }


        internal ConcreteGraphicsContext(D3D11.DeviceContext d3dContext) : base()
        {
            _d3dContext = d3dContext;

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                if (_d3dContext != null)
                    _d3dContext.Dispose();
                _d3dContext = null;

                base.Dispose();
            }
        }

    }
}
