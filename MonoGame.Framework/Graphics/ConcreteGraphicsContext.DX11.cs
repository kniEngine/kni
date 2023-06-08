// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {
        private D3D11.DeviceContext _d3dContext;

        internal D3D11.DeviceContext D3dContext { get { return _d3dContext; } }


        internal ConcreteGraphicsContext(GraphicsDevice device, D3D11.DeviceContext d3dContext)
            : base(device)
        {
            _d3dContext = d3dContext;

        }

        internal static SharpDX.Mathematics.Interop.RawColor4 ToDXColor(Color blendFactor)
        {
            return new SharpDX.Mathematics.Interop.RawColor4(
                    blendFactor.R / 255.0f,
                    blendFactor.G / 255.0f,
                    blendFactor.B / 255.0f,
                    blendFactor.A / 255.0f);
        }


        internal static PrimitiveTopology ToPrimitiveTopology(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return PrimitiveTopology.LineList;
                case PrimitiveType.LineStrip:
                    return PrimitiveTopology.LineStrip;
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;
                case PrimitiveType.TriangleStrip:
                    return PrimitiveTopology.TriangleStrip;
                case PrimitiveType.PointList:
                    return PrimitiveTopology.PointList;

                default:
                    throw new ArgumentException();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                if (_d3dContext != null)
                    _d3dContext.Dispose();
                _d3dContext = null;
            }

            base.Dispose(disposing);
        }

    }
}
