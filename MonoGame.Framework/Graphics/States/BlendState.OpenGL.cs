// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            var blendEnabled = !(this.ColorSourceBlend == Blend.One &&
                                 this.ColorDestinationBlend == Blend.Zero &&
                                 this.AlphaSourceBlend == Blend.One &&
                                 this.AlphaDestinationBlend == Blend.Zero);
            if (force || blendEnabled != device._lastBlendEnable)
            {
                if (blendEnabled)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GraphicsExtensions.CheckGLError();
                device._lastBlendEnable = blendEnabled;
            }
            if (_independentBlendEnable)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (force ||
                        _targetBlendState[i].ColorBlendFunction != device._lastBlendState[i].ColorBlendFunction ||
                        _targetBlendState[i].AlphaBlendFunction != device._lastBlendState[i].AlphaBlendFunction)
                    {
                        GL.BlendEquationSeparatei(i,
                            ToGLBlendEquationMode(_targetBlendState[i].ColorBlendFunction),
                            ToGLBlendEquationMode(_targetBlendState[i].AlphaBlendFunction));
                        GraphicsExtensions.CheckGLError();
                        device._lastBlendState[i].ColorBlendFunction = this._targetBlendState[i].ColorBlendFunction;
                        device._lastBlendState[i].AlphaBlendFunction = this._targetBlendState[i].AlphaBlendFunction;
                    }

                    if (force ||
                        _targetBlendState[i].ColorSourceBlend != device._lastBlendState[i].ColorSourceBlend ||
                        _targetBlendState[i].ColorDestinationBlend != device._lastBlendState[i].ColorDestinationBlend ||
                        _targetBlendState[i].AlphaSourceBlend != device._lastBlendState[i].AlphaSourceBlend ||
                        _targetBlendState[i].AlphaDestinationBlend != device._lastBlendState[i].AlphaDestinationBlend)
                    {
                        GL.BlendFuncSeparatei(i,
                            ToGLBlendFuncSrc(_targetBlendState[i].ColorSourceBlend),
                            ToGLBlendFuncDest(_targetBlendState[i].ColorDestinationBlend),
                            ToGLBlendFuncSrc(_targetBlendState[i].AlphaSourceBlend),
                            ToGLBlendFuncDest(_targetBlendState[i].AlphaDestinationBlend));
                        GraphicsExtensions.CheckGLError();
                        device._lastBlendState[i].ColorSourceBlend = _targetBlendState[i].ColorSourceBlend;
                        device._lastBlendState[i].ColorDestinationBlend = _targetBlendState[i].ColorDestinationBlend;
                        device._lastBlendState[i].AlphaSourceBlend = _targetBlendState[i].AlphaSourceBlend;
                        device._lastBlendState[i].AlphaDestinationBlend = _targetBlendState[i].AlphaDestinationBlend;
                    }
                }
            }
            else
            {
                if (force ||
                    this.ColorBlendFunction != device._lastBlendState.ColorBlendFunction ||
                    this.AlphaBlendFunction != device._lastBlendState.AlphaBlendFunction)
                {
                    GL.BlendEquationSeparate(
                        ToGLBlendEquationMode(this.ColorBlendFunction),
                        ToGLBlendEquationMode(this.AlphaBlendFunction));
                    GraphicsExtensions.CheckGLError();
                    for (int i = 0; i < 4; i++)
                    {
                        device._lastBlendState[i].ColorBlendFunction = this.ColorBlendFunction;
                        device._lastBlendState[i].AlphaBlendFunction = this.AlphaBlendFunction;
                    }
                }

                if (force ||
                    this.ColorSourceBlend != device._lastBlendState.ColorSourceBlend ||
                    this.ColorDestinationBlend != device._lastBlendState.ColorDestinationBlend ||
                    this.AlphaSourceBlend != device._lastBlendState.AlphaSourceBlend ||
                    this.AlphaDestinationBlend != device._lastBlendState.AlphaDestinationBlend)
                {
                    GL.BlendFuncSeparate(
                        ToGLBlendFuncSrc(this.ColorSourceBlend),
                        ToGLBlendFuncDest(this.ColorDestinationBlend),
                        ToGLBlendFuncSrc(this.AlphaSourceBlend),
                        ToGLBlendFuncDest(this.AlphaDestinationBlend));
                    GraphicsExtensions.CheckGLError();
                    for (int i = 0; i < 4; i++)
                    {
                        device._lastBlendState[i].ColorSourceBlend = this.ColorSourceBlend;
                        device._lastBlendState[i].ColorDestinationBlend = this.ColorDestinationBlend;
                        device._lastBlendState[i].AlphaSourceBlend = this.AlphaSourceBlend;
                        device._lastBlendState[i].AlphaDestinationBlend = this.AlphaDestinationBlend;
                    }
                }
            }

            if (force || this.ColorWriteChannels != device._lastBlendState.ColorWriteChannels)
            {
                GL.ColorMask(
                    (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                GraphicsExtensions.CheckGLError();
                device._lastBlendState.ColorWriteChannels = this.ColorWriteChannels;
            }
        }
        

        private static BlendEquationMode ToGLBlendEquationMode(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;

#if DESKTOPGL || IOS || TVOS
                case BlendFunction.Max:
                    return BlendEquationMode.Max;
                case BlendFunction.Min:
                    return BlendEquationMode.Min;
#endif

                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;

                default:
                    throw new ArgumentException();
            }
        }

        private static BlendingFactorSrc ToGLBlendFuncSrc(Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorSrc.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;

                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }
        }

        private static BlendingFactorDest ToGLBlendFuncDest(Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return BlendingFactorDest.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFactorDest.DstColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorDest.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFactorDest.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorDest.One;
                case Blend.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorDest.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case Blend.Zero:
                    return BlendingFactorDest.Zero;

                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }
        }

    }
}

