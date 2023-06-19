// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(GraphicsContextStrategy context, bool force = false)
        {
            var GL = this.GraphicsDevice._glContext;

            if (force ||
                this.DepthBufferEnable != ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(WebGLCapability.DEPTH_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    GL.Enable(WebGLCapability.DEPTH_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force ||
                this.DepthBufferFunction != ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(GraphicsExtensions.ToGLComparisonFunction(DepthBufferFunction));
                GraphicsExtensions.CheckGLError();
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if (force ||
                this.DepthBufferWriteEnable != ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferWriteEnable)
            {
                GL.DepthMask(DepthBufferWriteEnable);
                GraphicsExtensions.CheckGLError();
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if (force ||
                this.StencilEnable != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilEnable)
            {
                if (!StencilEnable)
                {
                    GL.Disable(WebGLCapability.STENCIL_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    GL.Enable(WebGLCapability.STENCIL_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFunc(GraphicsExtensions.ToGLComparisonFunction(this.StencilFunction), ReferenceStencil, StencilMask);
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFail ||
                    this.StencilDepthBufferFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilDepthBufferFail ||
                    this.StencilPass != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOp(ToGLStencilOp(StencilFail),
                                 ToGLStencilOp(StencilDepthBufferFail),
                                 ToGLStencilOp(StencilPass));
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFail = this.StencilFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;

            if (force ||
                this.StencilWriteMask != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilWriteMask)
            {
                GL.StencilMask(this.StencilWriteMask);
                GraphicsExtensions.CheckGLError();
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
            }
        }

        private static WebGLStencilOpFunc ToGLStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return WebGLStencilOpFunc.KEEP;
                case StencilOperation.Decrement:
                    return WebGLStencilOpFunc.DECR_WRAP;
                case StencilOperation.DecrementSaturation:
                    return WebGLStencilOpFunc.DECR;
                case StencilOperation.IncrementSaturation:
                    return WebGLStencilOpFunc.INCR;
                case StencilOperation.Increment:
                    return WebGLStencilOpFunc.INCR_WRAP;
                case StencilOperation.Invert:
                    return WebGLStencilOpFunc.INVERT;
                case StencilOperation.Replace:
                    return WebGLStencilOpFunc.REPLACE;
                case StencilOperation.Zero:
                    return WebGLStencilOpFunc.ZERO;

                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }
    }
}

