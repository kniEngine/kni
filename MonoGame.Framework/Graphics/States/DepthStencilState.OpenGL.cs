// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(GraphicsContextStrategy context, GraphicsDevice device, bool force = false)
        {
            if (force ||
                this.DepthBufferEnable != ((ConcreteGraphicsContext)context)._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(EnableCap.DepthTest);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    GL.Enable(EnableCap.DepthTest);
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
                    GL.Disable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    GL.Enable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();
                }
                ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                StencilFace cullFaceModeFront = StencilFace.Front;
                StencilFace cullFaceModeBack = StencilFace.Back;
                StencilFace stencilFaceFront = StencilFace.Front;
                StencilFace stencilFaceBack = StencilFace.Back;

                if (force ||
					this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFuncSeparate(cullFaceModeFront, GraphicsExtensions.ToGLComparisonFunction(this.StencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFunction != ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilFunction ||
                    this.ReferenceStencil != ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil ||
                    this.StencilMask != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask)
			    {
                    GL.StencilFuncSeparate(cullFaceModeBack, GraphicsExtensions.ToGLComparisonFunction(this.CounterClockwiseStencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilFunction = this.CounterClockwiseStencilFunction;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                
                if (force ||
					this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFail ||
					this.StencilDepthBufferFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilDepthBufferFail ||
					this.StencilPass != ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOpSeparate(stencilFaceFront, ToGLStencilOp(this.StencilFail),
                                         ToGLStencilOp(this.StencilDepthBufferFail),
                                         ToGLStencilOp(this.StencilPass));
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilFail = this.StencilFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.StencilPass = this.StencilPass;
                }

                if (force ||
                    this.TwoSidedStencilMode != ((ConcreteGraphicsContext)context)._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilFail ||
                    this.CounterClockwiseStencilDepthBufferFail != ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail ||
                    this.CounterClockwiseStencilPass != ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilPass)
			    {
                    GL.StencilOpSeparate(stencilFaceBack, ToGLStencilOp(this.CounterClockwiseStencilFail),
                                         ToGLStencilOp(this.CounterClockwiseStencilDepthBufferFail),
                                         ToGLStencilOp(this.CounterClockwiseStencilPass));
                    GraphicsExtensions.CheckGLError();
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilFail = this.CounterClockwiseStencilFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail = this.CounterClockwiseStencilDepthBufferFail;
                    ((ConcreteGraphicsContext)context)._lastDepthStencilState.CounterClockwiseStencilPass = this.CounterClockwiseStencilPass;
                }
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

