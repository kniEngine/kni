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

            if (!this.IndependentBlendEnable)
            {
                bool blendEnabled = !(this.ColorSourceBlend == Blend.One &&
                                      this.ColorDestinationBlend == Blend.Zero &&
                                      this.AlphaSourceBlend == Blend.One &&
                                      this.AlphaDestinationBlend == Blend.Zero);

                int blendEnabledMask = blendEnabled ? 0xF : 0x0;

                if (force ||
                    blendEnabledMask != context._lastBlendEnableMask)
                {
                    if (blendEnabled)
                        GL.Enable(WebGLCapability.BLEND);
                    else
                        GL.Disable(WebGLCapability.BLEND);
                    GL.CheckGLError();
                    context._lastBlendEnableMask = blendEnabledMask;
                }
            }
            else
            {
                if (!((ConcreteGraphicsCapabilities)context.Capabilities).SupportsSeparateBlendStates)
                    throw new PlatformNotSupportedException("Independent blend states on BlazorGL require WebGL2 and the OES_draw_buffers_indexed extension.");

                int newBlendEnableMask = 0;
                for (int i = 0; i < this.Targets.Length; i++)
                {
                    bool blendEnabled = !(this.Targets[i].ColorSourceBlend == Blend.One &&
                                          this.Targets[i].ColorDestinationBlend == Blend.Zero &&
                                          this.Targets[i].AlphaSourceBlend == Blend.One &&
                                          this.Targets[i].AlphaDestinationBlend == Blend.Zero);

                    int targetMask = blendEnabled ? (1 << i) : 0;
                    int lastTargetMask = context._lastBlendEnableMask & (1 << i);

                    if (force ||
                        targetMask != lastTargetMask)
                    {
                        if (blendEnabled)
                            ((IWebGL2RenderingContext)GL).DrawBuffersIndexedExtension.Enablei(WebGLCapability.BLEND, i);
                        else
                            ((IWebGL2RenderingContext)GL).DrawBuffersIndexedExtension.Disablei(WebGLCapability.BLEND, i);
                        GL.CheckGLError();
                    }
                    newBlendEnableMask |= targetMask;
                }
                context._lastBlendEnableMask = newBlendEnableMask;
            }

            if (!((ConcreteGraphicsCapabilities)context.Capabilities).SupportsBlendStateMinMax)
            {
                for (int i = 0; i < this.Targets.Length; i++)
                {
                    if (this.Targets[i].ColorBlendFunction == BlendFunction.Min || this.Targets[i].ColorBlendFunction == BlendFunction.Max ||
                        this.Targets[i].AlphaBlendFunction == BlendFunction.Min || this.Targets[i].AlphaBlendFunction == BlendFunction.Max)
                        throw new PlatformNotSupportedException("BlendFunction Min/Max on BlazorGL requires WebGL2 or the EXT_blend_minmax extension.");

                    if (!this.IndependentBlendEnable)
                        break;
                }
            }

            if (!this.IndependentBlendEnable)
            {
                bool targetsFunctionsDiffer = false;
                bool targetsBlendsDiffer = false;
                bool targetsChannelsDiffer = false;
                if (context._lastBlendState.IndependentBlendEnable)
                {
                    for (int i = 0; i < this.Targets.Length; i++)
                    {
                        if (this.ColorBlendFunction != context._lastBlendState[i].ColorBlendFunction ||
                            this.AlphaBlendFunction != context._lastBlendState[i].AlphaBlendFunction)
                            targetsFunctionsDiffer = true;

                        if (this.ColorSourceBlend != context._lastBlendState[i].ColorSourceBlend ||
                            this.ColorDestinationBlend != context._lastBlendState[i].ColorDestinationBlend ||
                            this.AlphaSourceBlend != context._lastBlendState[i].AlphaSourceBlend ||
                            this.AlphaDestinationBlend != context._lastBlendState[i].AlphaDestinationBlend)
                            targetsBlendsDiffer = true;

                        if (this.ColorWriteChannels != context._lastBlendState[i].ColorWriteChannels)
                            targetsChannelsDiffer = true;
                    }
                }

                if (force || targetsFunctionsDiffer ||
                    this.ColorBlendFunction != context._lastBlendState.ColorBlendFunction ||
                    this.AlphaBlendFunction != context._lastBlendState.AlphaBlendFunction)
                {
                    GL.BlendEquationSeparate(
                        ToGLBlendEquationMode(this.ColorBlendFunction),
                        ToGLBlendEquationMode(this.AlphaBlendFunction));
                    GL.CheckGLError();
                    for (int i = 0; i < this.Targets.Length; i++)
                    {
                        context._lastBlendState[i].ColorBlendFunction = this.ColorBlendFunction;
                        context._lastBlendState[i].AlphaBlendFunction = this.AlphaBlendFunction;
                    }
                }

                if (force || targetsBlendsDiffer ||
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
                    for (int i = 0; i < this.Targets.Length; i++)
                    {
                        context._lastBlendState[i].ColorSourceBlend = this.ColorSourceBlend;
                        context._lastBlendState[i].ColorDestinationBlend = this.ColorDestinationBlend;
                        context._lastBlendState[i].AlphaSourceBlend = this.AlphaSourceBlend;
                        context._lastBlendState[i].AlphaDestinationBlend = this.AlphaDestinationBlend;
                    }
                }

                if (force || targetsChannelsDiffer ||
                    this.ColorWriteChannels != context._lastBlendState.ColorWriteChannels)
                {
                    GL.ColorMask(
                        (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                        (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                        (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                        (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                    GL.CheckGLError();
                    for (int i = 0; i < this.Targets.Length; i++)
                        context._lastBlendState[i].ColorWriteChannels = this.ColorWriteChannels;
                }
            }
            else // (_strategy.IndependentBlendEnable == true)
            {
                for (int i = 0; i < this.Targets.Length; i++)
                {
                    if (force ||
                        this.Targets[i].ColorBlendFunction != context._lastBlendState[i].ColorBlendFunction ||
                        this.Targets[i].AlphaBlendFunction != context._lastBlendState[i].AlphaBlendFunction)
                    {
                        ((IWebGL2RenderingContext)GL).DrawBuffersIndexedExtension.BlendEquationSeparatei(i,
                            ToGLBlendEquationMode(this.Targets[i].ColorBlendFunction),
                            ToGLBlendEquationMode(this.Targets[i].AlphaBlendFunction));
                        GL.CheckGLError();
                        context._lastBlendState[i].ColorBlendFunction = this.Targets[i].ColorBlendFunction;
                        context._lastBlendState[i].AlphaBlendFunction = this.Targets[i].AlphaBlendFunction;
                    }

                    if (force ||
                        this.Targets[i].ColorSourceBlend != context._lastBlendState[i].ColorSourceBlend ||
                        this.Targets[i].ColorDestinationBlend != context._lastBlendState[i].ColorDestinationBlend ||
                        this.Targets[i].AlphaSourceBlend != context._lastBlendState[i].AlphaSourceBlend ||
                        this.Targets[i].AlphaDestinationBlend != context._lastBlendState[i].AlphaDestinationBlend)
                    {
                        ((IWebGL2RenderingContext)GL).DrawBuffersIndexedExtension.BlendFuncSeparatei(i,
                            ToGLBlendFunc(this.Targets[i].ColorSourceBlend),
                            ToGLBlendFunc(this.Targets[i].ColorDestinationBlend),
                            ToGLBlendFunc(this.Targets[i].AlphaSourceBlend),
                            ToGLBlendFunc(this.Targets[i].AlphaDestinationBlend));
                        GL.CheckGLError();
                        context._lastBlendState[i].ColorSourceBlend = this.Targets[i].ColorSourceBlend;
                        context._lastBlendState[i].ColorDestinationBlend = this.Targets[i].ColorDestinationBlend;
                        context._lastBlendState[i].AlphaSourceBlend = this.Targets[i].AlphaSourceBlend;
                        context._lastBlendState[i].AlphaDestinationBlend = this.Targets[i].AlphaDestinationBlend;
                    }

                    if (force ||
                        this.Targets[i].ColorWriteChannels != context._lastBlendState[i].ColorWriteChannels)
                    {
                        ((IWebGL2RenderingContext)GL).DrawBuffersIndexedExtension.ColorMaski(i,
                            (this.Targets[i].ColorWriteChannels & ColorWriteChannels.Red) != 0,
                            (this.Targets[i].ColorWriteChannels & ColorWriteChannels.Green) != 0,
                            (this.Targets[i].ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                            (this.Targets[i].ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                        GL.CheckGLError();
                        context._lastBlendState[i].ColorWriteChannels = this.Targets[i].ColorWriteChannels;
                    }
                }
            }

            context._lastBlendState.IndependentBlendEnable = this.IndependentBlendEnable;
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
                case BlendFunction.Min:
                    return WebGLEquationFunc.MIN;
                case BlendFunction.Max:
                    return WebGLEquationFunc.MAX;

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

        protected override void PlatformGraphicsContextLost()
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
