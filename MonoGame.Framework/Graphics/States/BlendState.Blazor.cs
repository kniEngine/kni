// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            var GL = device._glContext;

            bool blendEnabled = !(this.ColorSourceBlend == Blend.One &&
                                  this.ColorDestinationBlend == Blend.Zero &&
                                  this.AlphaSourceBlend == Blend.One &&
                                  this.AlphaDestinationBlend == Blend.Zero);

            if (force ||
                blendEnabled != device._lastBlendEnable)
            {
                if (blendEnabled)
                    GL.Enable(WebGLCapability.BLEND);
                else
                    GL.Disable(WebGLCapability.BLEND);
                GraphicsExtensions.CheckGLError();
                device._lastBlendEnable = blendEnabled;
            }

            if (!_independentBlendEnable)
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
                        ToGLBlendFunc(this.ColorSourceBlend),
                        ToGLBlendFunc(this.ColorDestinationBlend),
                        ToGLBlendFunc(this.AlphaSourceBlend),
                        ToGLBlendFunc(this.AlphaDestinationBlend));
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
            else
            {
                throw new NotImplementedException();
            }

            if (force ||
                this.ColorWriteChannels != device._lastBlendState.ColorWriteChannels)
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
    }
}

