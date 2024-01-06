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
        private bool _inBeginEndPair;  // true if Begin was called and End was not yet called.
        private bool _queryPerformed;  // true if Begin+End were called at least once.
        private bool _isComplete;      // true if the result is available in _pixelCount.
        private int _pixelCount;       // The query result.

        private D3D11.Query _query;


        public override int PixelCount { get { return _pixelCount; } }

        public override bool IsComplete
        {
            get
            {
                if (_isComplete)
                    return true;

                if (!_queryPerformed || _inBeginEndPair)
                    return false;

                return PlatformGetResult();

                return _isComplete;
            }
        }


        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            //if (graphicsDevice.D3DDevice.FeatureLevel == D3D.FeatureLevel.Level_9_1)
            //    throw new NotSupportedException("The Reach profile does not support occlusion queries.");

            D3D11.QueryDescription queryDesc = new D3D11.QueryDescription();
            queryDesc.Flags = D3D11.QueryFlags.None;
            queryDesc.Type = D3D11.QueryType.Occlusion;
            _query = new D3D11.Query(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, queryDesc);
        }

        public override void PlatformBegin()
        {
            if (_inBeginEndPair)
                throw new InvalidOperationException("End() must be called before calling Begin() again.");

            _inBeginEndPair = true;
            _isComplete = false;

            lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.Begin(_query);
            }
        }

        public override void PlatformEnd()
        {
            if (!_inBeginEndPair)
                throw new InvalidOperationException("Begin() must be called before calling End().");

            _inBeginEndPair = false;
            _queryPerformed = true;

            lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.End(_query);
            }
        }

        private bool PlatformGetResult()
        {
            lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                ulong count;
                _isComplete = d3dContext.GetData(_query, out count);
                _pixelCount = (int)count;
            }

            return _isComplete;
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
