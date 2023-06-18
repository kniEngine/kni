// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsContextGL : GraphicsContextStrategy
    {

        internal ConcreteGraphicsContextGL(GraphicsDevice device)
            : base(device)
        {

        }

        internal void PlatformApplyBlend()
        {
            if (_blendStateDirty)
            {
                _actualBlendState.PlatformApplyState(this, this.Device);
                _blendStateDirty = false;
            }

            if (_blendFactorDirty)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();

                _blendFactorDirty = false;
            }
        }

        internal void PlatformApplyScissorRectangle()
        {
            Rectangle scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = this.Device.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
            _scissorRectangleDirty = false;
        }

        internal static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return GLPrimitiveType.Points;
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
                default:
                    throw new ArgumentException();
            }
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
