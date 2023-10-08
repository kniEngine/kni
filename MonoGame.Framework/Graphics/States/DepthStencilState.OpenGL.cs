// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(ConcreteGraphicsContextGL context, bool force = false)
        {
            var GL = OGL.Current;

            if (force ||
                this.DepthBufferEnable != context._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(EnableCap.DepthTest);
                    GL.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    GL.Enable(EnableCap.DepthTest);
                    GL.CheckGLError();
                }
                context._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force || 
                this.DepthBufferFunction != context._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(GraphicsExtensions.ToGLComparisonFunction(DepthBufferFunction));
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
                    GL.Disable(EnableCap.StencilTest);
                    GL.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    GL.Enable(EnableCap.StencilTest);
                    GL.CheckGLError();
                }
                context._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                StencilFace cullFaceModeFront = StencilFace.Front;
                StencilFace cullFaceModeBack = StencilFace.Back;
                StencilFace stencilFaceFront = StencilFace.Front;
                StencilFace stencilFaceBack = StencilFace.Back;

                if (force ||
					this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != context._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != context._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != context._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFuncSeparate(cullFaceModeFront, GraphicsExtensions.ToGLComparisonFunction(this.StencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GL.CheckGLError();
                    context._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    context._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    context._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFunction != context._lastDepthStencilState.CounterClockwiseStencilFunction ||
                    this.ReferenceStencil != context._lastDepthStencilState.ReferenceStencil ||
                    this.StencilMask != context._lastDepthStencilState.StencilMask)
			    {
                    GL.StencilFuncSeparate(cullFaceModeBack, GraphicsExtensions.ToGLComparisonFunction(this.CounterClockwiseStencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GL.CheckGLError();
                    context._lastDepthStencilState.CounterClockwiseStencilFunction = this.CounterClockwiseStencilFunction;
                    context._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    context._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                
                if (force ||
					this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFail != context._lastDepthStencilState.StencilFail ||
					this.StencilDepthBufferFail != context._lastDepthStencilState.StencilDepthBufferFail ||
					this.StencilPass != context._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOpSeparate(stencilFaceFront, ToGLStencilOp(this.StencilFail),
                                         ToGLStencilOp(this.StencilDepthBufferFail),
                                         ToGLStencilOp(this.StencilPass));
                    GL.CheckGLError();
                    context._lastDepthStencilState.StencilFail = this.StencilFail;
                    context._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    context._lastDepthStencilState.StencilPass = this.StencilPass;
                }

                if (force ||
                    this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFail != context._lastDepthStencilState.CounterClockwiseStencilFail ||
                    this.CounterClockwiseStencilDepthBufferFail != context._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail ||
                    this.CounterClockwiseStencilPass != context._lastDepthStencilState.CounterClockwiseStencilPass)
			    {
                    GL.StencilOpSeparate(stencilFaceBack, ToGLStencilOp(this.CounterClockwiseStencilFail),
                                         ToGLStencilOp(this.CounterClockwiseStencilDepthBufferFail),
                                         ToGLStencilOp(this.CounterClockwiseStencilPass));
                    GL.CheckGLError();
                    context._lastDepthStencilState.CounterClockwiseStencilFail = this.CounterClockwiseStencilFail;
                    context._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail = this.CounterClockwiseStencilDepthBufferFail;
                    context._lastDepthStencilState.CounterClockwiseStencilPass = this.CounterClockwiseStencilPass;
                }
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != context._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != context._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != context._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != context._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFunc(GraphicsExtensions.ToGLComparisonFunction(this.StencilFunction), ReferenceStencil, StencilMask);
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

        private static StencilOp ToGLStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Decrement:
                    return StencilOp.DecrWrap;
                case StencilOperation.DecrementSaturation:
                    return StencilOp.Decr;
                case StencilOperation.IncrementSaturation:
                    return StencilOp.Incr;
                case StencilOperation.Increment:
                    return StencilOp.IncrWrap;
                case StencilOperation.Invert:
                    return StencilOp.Invert;
                case StencilOperation.Replace:
                    return StencilOp.Replace;
                case StencilOperation.Zero:
                    return StencilOp.Zero;

                default:
                    return StencilOp.Keep;
            }
        }
    }
}

