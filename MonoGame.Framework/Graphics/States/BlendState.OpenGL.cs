// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
                            _targetBlendState[i].ColorBlendFunction.ToGLBlendEquationMode(),
                            _targetBlendState[i].AlphaBlendFunction.ToGLBlendEquationMode());
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
                            _targetBlendState[i].ColorSourceBlend.ToGLBlendFuncSrc(),
                            _targetBlendState[i].ColorDestinationBlend.ToGLBlendFuncDest(),
                            _targetBlendState[i].AlphaSourceBlend.ToGLBlendFuncSrc(),
                            _targetBlendState[i].AlphaDestinationBlend.ToGLBlendFuncDest());
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
                        this.ColorBlendFunction.ToGLBlendEquationMode(),
                        this.AlphaBlendFunction.ToGLBlendEquationMode());
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
                        this.ColorSourceBlend.ToGLBlendFuncSrc(),
                        this.ColorDestinationBlend.ToGLBlendFuncDest(),
                        this.AlphaSourceBlend.ToGLBlendFuncSrc(),
                        this.AlphaDestinationBlend.ToGLBlendFuncDest());
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
    }
}

