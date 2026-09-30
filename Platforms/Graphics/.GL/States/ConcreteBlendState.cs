// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteBlendState : ResourceBlendStateStrategy
    {

        internal ConcreteBlendState(GraphicsContextStrategy contextStrategy, IBlendStateStrategy source)
            : base(contextStrategy, source)
        {
        }

        internal void PlatformApplyState(ConcreteGraphicsContextGL context, bool force = false)
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
                        GL.Enable(EnableCap.Blend);
                    else
                        GL.Disable(EnableCap.Blend);
                    GL.CheckGLError();
                    context._lastBlendEnableMask = blendEnabledMask;
                }
            }
            else
            {
                if (!((ConcreteGraphicsCapabilities)context.Capabilities).SupportsSeparateBlendStates)
                    throw new PlatformNotSupportedException("Independent blend states require at least OpenGL 4.0 or GLES 3.2, or equivalent extension support.");

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
                            GL.Enablei(EnableCap.Blend, i);
                        else
                            GL.Disablei(EnableCap.Blend, i);
                        GL.CheckGLError();
                    }
                    newBlendEnableMask |= targetMask;
                }
                context._lastBlendEnableMask = newBlendEnableMask;
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
                        GL.BlendEquationSeparatei(i,
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
                        GL.BlendFuncSeparatei(i,
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
                        GL.ColorMaski(i,
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

        private static BlendEquationMode ToGLBlendEquationMode(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;

#if DESKTOPGL || IOS || TVOS
                case BlendFunction.Max:
                    return BlendEquationMode.Max;
                case BlendFunction.Min:
                    return BlendEquationMode.Min;
#endif

                default:
                    throw new ArgumentException();
            }
        }

        private static BlendingFunc ToGLBlendFunc(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return BlendingFunc.Zero;
                case Blend.One:
                    return BlendingFunc.One;
                case Blend.BlendFactor:
                    return BlendingFunc.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendingFunc.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFunc.DstColor;
                case Blend.InverseBlendFactor:
                    return BlendingFunc.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFunc.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFunc.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFunc.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFunc.OneMinusSrcColor;
                case Blend.SourceAlpha:
                    return BlendingFunc.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFunc.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFunc.SrcColor;

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
