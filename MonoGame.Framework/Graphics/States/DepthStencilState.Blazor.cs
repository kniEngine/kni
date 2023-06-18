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
        internal void PlatformApplyState(GraphicsContextStrategy context, GraphicsDevice device, bool force = false)
        {
            var GL = device._glContext;

            if (force ||
                this.DepthBufferEnable != device._lastDepthStencilState.DepthBufferEnable)
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
                device._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force ||
                this.DepthBufferFunction != device._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(GraphicsExtensions.ToGLComparisonFunction(DepthBufferFunction));
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if (force ||
                this.DepthBufferWriteEnable != device._lastDepthStencilState.DepthBufferWriteEnable)
            {
                GL.DepthMask(DepthBufferWriteEnable);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if (force ||
                this.StencilEnable != device._lastDepthStencilState.StencilEnable)
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
                device._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != device._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFunc(GraphicsExtensions.ToGLComparisonFunction(this.StencilFunction), ReferenceStencil, StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != device._lastDepthStencilState.StencilFail ||
                    this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail ||
                    this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOp(ToGLStencilOp(StencilFail),
                                 ToGLStencilOp(StencilDepthBufferFail),
                                 ToGLStencilOp(StencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            device._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;

            if (force ||
                this.StencilWriteMask != device._lastDepthStencilState.StencilWriteMask)
            {
                GL.StencilMask(this.StencilWriteMask);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
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

