// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class OcclusionQuery
    {
        private int _glQueryId = -1;

        private void PlatformConstructOcclusionQuery()
        {
            _glQueryId = GL.GenQuery();
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformBegin()
        {
            GL.BeginQuery(QueryTarget.SamplesPassed, _glQueryId);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformEnd()
        {
            GL.EndQuery(QueryTarget.SamplesPassed);
            GraphicsExtensions.CheckGLError();
        }

        private bool PlatformGetResult(out int pixelCount)
        {
            int resultReady = 0;
            GL.GetQueryObject(_glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
            GraphicsExtensions.CheckGLError();

            if (resultReady == 0)
            {
                pixelCount = 0;
                return false;
            }

            GL.GetQueryObject(_glQueryId, GetQueryObjectParam.QueryResult, out pixelCount);
            GraphicsExtensions.CheckGLError();

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                if (_strategy != null)
                    _strategy.Dispose();
            }

            if (_glQueryId > -1)
            {
                if (!GraphicsDevice.IsDisposed)
                {
                    GL.DeleteQuery(_glQueryId);
                    GraphicsExtensions.CheckGLError();
                }
                _glQueryId = -1;
            }

            base.Dispose(disposing);
        }
    }
}

