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
        internal void PlatformApplyState(ConcreteGraphicsContext context, bool force = false)
        {
            var GL = context.GL;

            if (force ||
                this.DepthBufferEnable != context._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(WebGLCapability.DEPTH_TEST);
                    GL.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    GL.Enable(WebGLCapability.DEPTH_TEST);
                    GL.CheckGLError();
                }
                context._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force ||
                this.DepthBufferFunction != context._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(DepthBufferFunction.ToGLComparisonFunction());
                GL.CheckGLError();
                context._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if (force ||
                this.DepthBufferWriteEnable != context._lastDepthStencilState.DepthBufferWriteEnable)
            {
                GL.DepthMask(DepthBufferWriteEnable);
                GL.CheckGLError();
                context._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if (force ||
                this.StencilEnable != context._lastDepthStencilState.StencilEnable)
            {
                if (!StencilEnable)
                {
                    GL.Disable(WebGLCapability.STENCIL_TEST);
                    GL.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    GL.Enable(WebGLCapability.STENCIL_TEST);
                    GL.CheckGLError();
                }
                context._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != context._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != context._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != context._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFunc(this.StencilFunction.ToGLComparisonFunction(), ReferenceStencil, StencilMask);
                    GL.CheckGLError();
                    context._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    context._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    context._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != context._lastDepthStencilState.StencilFail ||
                    this.StencilDepthBufferFail != context._lastDepthStencilState.StencilDepthBufferFail ||
                    this.StencilPass != context._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOp(ToGLStencilOp(StencilFail),
                                 ToGLStencilOp(StencilDepthBufferFail),
                                 ToGLStencilOp(StencilPass));
                    GL.CheckGLError();
                    context._lastDepthStencilState.StencilFail = this.StencilFail;
                    context._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    context._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            context._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;

            if (force ||
                this.StencilWriteMask != context._lastDepthStencilState.StencilWriteMask)
            {
                GL.StencilMask(this.StencilWriteMask);
                GL.CheckGLError();
                context._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
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

