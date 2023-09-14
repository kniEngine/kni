// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {
        private D3D11.Query _query;

        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            //if (graphicsDevice.D3DDevice.FeatureLevel == D3D.FeatureLevel.Level_9_1)
            //    throw new NotSupportedException("The Reach profile does not support occlusion queries.");

            D3D11.QueryDescription queryDesc = new D3D11.QueryDescription();
            queryDesc.Flags = D3D11.QueryFlags.None;
            queryDesc.Type = D3D11.QueryType.Occlusion;
            _query = new D3D11.Query(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, queryDesc);
        }

        public override void PlatformBegin()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.Begin(_query);
            }
        }

        public override void PlatformEnd()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.End(_query);
            }
        }

        public override bool PlatformGetResult(out int pixelCount)
        {
            ulong count;
            bool isComplete;

            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                isComplete = d3dContext.GetData(_query, out count);
            }

            pixelCount = (int)count;
            return isComplete;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _query.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
