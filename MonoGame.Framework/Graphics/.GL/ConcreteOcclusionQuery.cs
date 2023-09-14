// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {
        private int _glQueryId = -1;

        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            _glQueryId = GL.GenQuery();
            GraphicsExtensions.CheckGLError();
        }

        public override void PlatformBegin()
        {
            GL.BeginQuery(QueryTarget.SamplesPassed, _glQueryId);
            GraphicsExtensions.CheckGLError();
        }

        public override void PlatformEnd()
        {
            GL.EndQuery(QueryTarget.SamplesPassed);
            GraphicsExtensions.CheckGLError();
        }

        public override bool PlatformGetResult(out int pixelCount)
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
            if (disposing)
            {
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
