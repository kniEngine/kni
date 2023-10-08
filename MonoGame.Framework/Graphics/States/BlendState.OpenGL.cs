// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        internal void PlatformApplyState(ConcreteGraphicsContextGL context, bool force = false)
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
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GL.CheckGLError();
                context._lastBlendEnable = blendEnabled;
            }

            if (!_independentBlendEnable)
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
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (force ||
                        _targetBlendState[i].ColorBlendFunction != context._lastBlendState[i].ColorBlendFunction ||
                        _targetBlendState[i].AlphaBlendFunction != context._lastBlendState[i].AlphaBlendFunction)
                    {
                        GL.BlendEquationSeparatei(i,
                            ToGLBlendEquationMode(_targetBlendState[i].ColorBlendFunction),
                            ToGLBlendEquationMode(_targetBlendState[i].AlphaBlendFunction));
                        GL.CheckGLError();
                        context._lastBlendState[i].ColorBlendFunction = _targetBlendState[i].ColorBlendFunction;
                        context._lastBlendState[i].AlphaBlendFunction = _targetBlendState[i].AlphaBlendFunction;
                    }

                    if (force ||
                        _targetBlendState[i].ColorSourceBlend != context._lastBlendState[i].ColorSourceBlend ||
                        _targetBlendState[i].ColorDestinationBlend != context._lastBlendState[i].ColorDestinationBlend ||
                        _targetBlendState[i].AlphaSourceBlend != context._lastBlendState[i].AlphaSourceBlend ||
                        _targetBlendState[i].AlphaDestinationBlend != context._lastBlendState[i].AlphaDestinationBlend)
                    {
                        GL.BlendFuncSeparatei(i,
                            ToGLBlendFunc(_targetBlendState[i].ColorSourceBlend),
                            ToGLBlendFunc(_targetBlendState[i].ColorDestinationBlend),
                            ToGLBlendFunc(_targetBlendState[i].AlphaSourceBlend),
                            ToGLBlendFunc(_targetBlendState[i].AlphaDestinationBlend));
                        GL.CheckGLError();
                        context._lastBlendState[i].ColorSourceBlend = _targetBlendState[i].ColorSourceBlend;
                        context._lastBlendState[i].ColorDestinationBlend = _targetBlendState[i].ColorDestinationBlend;
                        context._lastBlendState[i].AlphaSourceBlend = _targetBlendState[i].AlphaSourceBlend;
                        context._lastBlendState[i].AlphaDestinationBlend = _targetBlendState[i].AlphaDestinationBlend;
                    }
                }
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
    }
}

