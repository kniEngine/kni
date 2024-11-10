// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {
        private bool _inBeginEndPair;  // true if Begin was called and End was not yet called.
        private bool _queryPerformed;  // true if Begin+End were called at least once.
        private bool _isComplete;      // true if the result is available in _pixelCount.
        private int _pixelCount;       // The query result.

        private int _glQueryId = -1;

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
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _glQueryId = GL.GenQuery();
            GL.CheckGLError();
        }

        public override void PlatformBegin()
        {
            if (_inBeginEndPair)
                throw new InvalidOperationException("End() must be called before calling Begin() again.");

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _inBeginEndPair = true;
            _isComplete = false;

            GL.BeginQuery(QueryTarget.SamplesPassed, _glQueryId);
            GL.CheckGLError();
        }

        public override void PlatformEnd()
        {
            if (!_inBeginEndPair)
                throw new InvalidOperationException("Begin() must be called before calling End().");

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            _inBeginEndPair = false;
            _queryPerformed = true;

            GL.EndQuery(QueryTarget.SamplesPassed);
            GL.CheckGLError();
        }

        private bool PlatformGetResult()
        {
            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int resultReady = 0;
            GL.GetQueryObject(_glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
            GL.CheckGLError();

            if (resultReady == 0)
            {
                _pixelCount = 0;
                _isComplete = false;
            }
            else
            {
                GL.GetQueryObject(_glQueryId, GetQueryObjectParam.QueryResult, out _pixelCount);
                GL.CheckGLError();
                _isComplete = true;
            }

            return _isComplete;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_glQueryId > -1)
            {
                if (!GraphicsDevice.IsDisposed)
                {
                    _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindSharedContext();
                    try
                    {
                        var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                        GL.DeleteQuery(_glQueryId);
                        GL.CheckGLError();
                    }
                    finally
                    {
                        _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindSharedContext();
                    }
                }
                _glQueryId = -1;
            }

            base.Dispose(disposing);
        }
    }
}
