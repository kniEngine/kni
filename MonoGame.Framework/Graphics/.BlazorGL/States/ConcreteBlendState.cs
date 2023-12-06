// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteBlendState : ResourceBlendStateStrategy
    {

        internal ConcreteBlendState(GraphicsContextStrategy contextStrategy, IBlendStateStrategy source)
            : base(contextStrategy, source)
        {
        }
        internal void PlatformApplyState(ConcreteGraphicsContext context, bool force = false)
        {
            var GL = context.GL;

            bool blendEnabled = !(this.ColorSourceBlend == Blend.One &&
                                  this.ColorDestinationBlend == Blend.Zero &&
                                  this.AlphaSourceBlend == Blend.One &&
                                  this.AlphaDestinationBlend == Blend.Zero);

            if (force ||
                blendEnabled != context._lastBlendEnable)
            {
                if (blendEnabled)
                    GL.Enable(WebGLCapability.BLEND);
                else
                    GL.Disable(WebGLCapability.BLEND);
                GL.CheckGLError();
                context._lastBlendEnable = blendEnabled;
            }

            if (!this.IndependentBlendEnable)
            {
                if (force ||
                    this.ColorBlendFunction != context._lastBlendState.ColorBlendFunction ||
                    this.AlphaBlendFunction != context._lastBlendState.AlphaBlendFunction)
                {
                    GL.BlendEquationSeparate(
                        ToGLBlendEquationMode(this.ColorBlendFunction),
                        ToGLBlendEquationMode(this.AlphaBlendFunction));
                    GL.CheckGLError();
                    for (int i = 0; i < 4; i++)
                    {
                        context._lastBlendState[i].ColorBlendFunction = this.ColorBlendFunction;
                        context._lastBlendState[i].AlphaBlendFunction = this.AlphaBlendFunction;
                    }
                }

                if (force ||
                    this.ColorSourceBlend != context._lastBlendState.ColorSourceBlend ||
                    this.ColorDestinationBlend != context._lastBlendState.ColorDestinationBlend ||
                    this.AlphaSourceBlend != context._lastBlendState.AlphaSourceBlend ||
                    this.AlphaDestinationBlend != context._lastBlendState.AlphaDestinationBlend)
                {
                    GL.BlendFuncSeparate(
                        ToGLBlendFunc(this.ColorSourceBlend),
                        ToGLBlendFunc(this.ColorDestinationBlend),
                        ToGLBlendFunc(this.AlphaSourceBlend),
                        ToGLBlendFunc(this.AlphaDestinationBlend));
                    GL.CheckGLError();
                    for (int i = 0; i < 4; i++)
                    {
                        context._lastBlendState[i].ColorSourceBlend = this.ColorSourceBlend;
                        context._lastBlendState[i].ColorDestinationBlend = this.ColorDestinationBlend;
                        context._lastBlendState[i].AlphaSourceBlend = this.AlphaSourceBlend;
                        context._lastBlendState[i].AlphaDestinationBlend = this.AlphaDestinationBlend;
                    }
                }
            }
            else // _strategy.IndependentBlendEnable == true
            {
                throw new NotImplementedException();
            }

            if (force ||
                this.ColorWriteChannels != context._lastBlendState.ColorWriteChannels)
            {
                GL.ColorMask(
                    (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                GL.CheckGLError();
                context._lastBlendState.ColorWriteChannels = this.ColorWriteChannels;
            }
        }

        private static WebGLEquationFunc ToGLBlendEquationMode(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return WebGLEquationFunc.ADD;
                case BlendFunction.ReverseSubtract:
                    return WebGLEquationFunc.REVERSE_SUBTRACT;
                case BlendFunction.Subtract:
                    return WebGLEquationFunc.SUBTRACT;

                default:
                    throw new ArgumentException();
            }
        }

        private static WebGLBlendFunc ToGLBlendFunc(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return WebGLBlendFunc.ZERO;
                case Blend.One:
                    return WebGLBlendFunc.ONE;
                case Blend.BlendFactor:
                    return WebGLBlendFunc.CONSTANT_COLOR;
                case Blend.DestinationAlpha:
                    return WebGLBlendFunc.DST_ALPHA;
                case Blend.DestinationColor:
                    return WebGLBlendFunc.DST_COLOR;
                case Blend.InverseBlendFactor:
                    return WebGLBlendFunc.ONE_MINUS_CONSTANT_COLOR;
                case Blend.InverseDestinationAlpha:
                    return WebGLBlendFunc.ONE_MINUS_DST_ALPHA;
                case Blend.InverseDestinationColor:
                    return WebGLBlendFunc.ONE_MINUS_DST_COLOR;
                case Blend.InverseSourceAlpha:
                    return WebGLBlendFunc.ONE_MINUS_SRC_ALPHA;
                case Blend.InverseSourceColor:
                    return WebGLBlendFunc.ONE_MINUS_SRC_COLOR;
                case Blend.SourceAlpha:
                    return WebGLBlendFunc.SRC_ALPHA;
                case Blend.SourceAlphaSaturation:
                    return WebGLBlendFunc.SRC_ALPHA_SATURATE;
                case Blend.SourceColor:
                    return WebGLBlendFunc.SRC_COLOR;

                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }
        }

        internal override void PlatformGraphicsContextLost()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }

}
