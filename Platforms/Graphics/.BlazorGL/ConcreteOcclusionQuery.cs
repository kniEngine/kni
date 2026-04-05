// Copyright (C)2023 Nick Kastellanos

using System;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {
        private bool _inBeginEndPair;  // true if Begin was called and End was not yet called.
        private bool _queryPerformed;  // true if Begin+End were called at least once.
        private bool _isComplete;      // true if the result is available in _pixelCount.
        private int _pixelCount;       // The query result.

        private WebGL2Query _query;

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
            }
        }

        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _query = ((IWebGL2RenderingContext)GL).CreateQuery();
            GL.CheckGLError();
        }

        public override void PlatformBegin()
        {
            if (_inBeginEndPair)
                throw new InvalidOperationException("End() must be called before calling Begin() again.");

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _inBeginEndPair = true;
            _isComplete = false;

            ((IWebGL2RenderingContext)GL).BeginQuery(WebGL2QueryType.ANY_SAMPLES_PASSED, _query);
            GL.CheckGLError();
        }

        public override void PlatformEnd()
        {
            if (!_inBeginEndPair)
                throw new InvalidOperationException("Begin() must be called before calling End().");

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            _inBeginEndPair = false;
            _queryPerformed = true;

            ((IWebGL2RenderingContext)GL).EndQuery(WebGL2QueryType.ANY_SAMPLES_PASSED);
            GL.CheckGLError();
        }

        private bool PlatformGetResult()
        {
            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int resultReady = ((IWebGL2RenderingContext)GL).GetQueryParameter(_query, WebGL2QueryParam.QUERY_RESULT_AVAILABLE);
            GL.CheckGLError();

            if (resultReady == 0)
            {
                _pixelCount = 0;
                _isComplete = false;
            }
            else
            {
                _pixelCount = ((IWebGL2RenderingContext)GL).GetQueryParameter(_query, WebGL2QueryParam.QUERY_RESULT);
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

            if (_query != null)
            {
                var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;
                _query.Dispose();
                GL.CheckGLError();
                _query = null;
            }

            base.Dispose(disposing);
        }
    }
}
