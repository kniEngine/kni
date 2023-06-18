// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        internal void PlatformApplyState(GraphicsContextStrategy context, GraphicsDevice device, bool force = false)
        {
            var GL = device._glContext;

            if (force)
            {
                // Turn off dithering to make sure data returned by Texture.GetData is accurate
                GL.Disable(WebGLCapability.DITHER);
            }

            // When rendering offscreen the faces change order.
            bool offscreen = context.IsRenderTargetBound;

            switch (CullMode)
            {
                case CullMode.None:
                    GL.Disable(WebGLCapability.CULL_FACE);
                    GraphicsExtensions.CheckGLError();
                    break;

                case Graphics.CullMode.CullClockwiseFace:
                    GL.Enable(WebGLCapability.CULL_FACE);
                    GraphicsExtensions.CheckGLError();
                    GL.CullFace(WebGLCullFaceMode.BACK);
                    GraphicsExtensions.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(WebGLWinding.CW);
                    else
                        GL.FrontFace(WebGLWinding.CCW);
                    GraphicsExtensions.CheckGLError();
                    break;

                case Graphics.CullMode.CullCounterClockwiseFace:
                    GL.Enable(WebGLCapability.CULL_FACE);
                    GraphicsExtensions.CheckGLError();
                    GL.CullFace(WebGLCullFaceMode.BACK);
                    GraphicsExtensions.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(WebGLWinding.CCW);
                    else
                        GL.FrontFace(WebGLWinding.CW);
                    GraphicsExtensions.CheckGLError();
                    break;

                default:
                    throw new InvalidOperationException("CullMode");
            }

            if (FillMode == FillMode.WireFrame)
                throw new PlatformNotSupportedException();

            if (force ||
                this.ScissorTestEnable != device._lastRasterizerState.ScissorTestEnable)
			{
			    if (ScissorTestEnable)
				    GL.Enable(WebGLCapability.SCISSOR_TEST);
			    else
				    GL.Disable(WebGLCapability.SCISSOR_TEST);
                GraphicsExtensions.CheckGLError();
                device._lastRasterizerState.ScissorTestEnable = this.ScissorTestEnable;
            }

            if (force || 
                this.DepthBias != device._lastRasterizerState.DepthBias ||
                this.SlopeScaleDepthBias != device._lastRasterizerState.SlopeScaleDepthBias)
            {
                if (this.DepthBias != 0 || this.SlopeScaleDepthBias != 0)
                {
                    // from the docs it seems this works the same as for Direct3D
                    // https://www.khronos.org/opengles/sdk/docs/man/xhtml/glPolygonOffset.xml
                    // explanation for Direct3D is  in https://github.com/MonoGame/MonoGame/issues/4826
                    DepthFormat activeDepthFormat = (context.IsRenderTargetBound)
                                                  ? context._currentRenderTargetBindings[0].DepthFormat
                                                  : device.PresentationParameters.DepthStencilFormat;
                    int depthMul;
                    switch (activeDepthFormat)
                    {
                        case DepthFormat.None:
                            depthMul = 0;
                            break;
                        case DepthFormat.Depth16:
                            depthMul = 1 << 16 - 1;
                            break;
                        case DepthFormat.Depth24:
                        case DepthFormat.Depth24Stencil8:
                            depthMul = 1 << 24 - 1;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    GL.Enable(WebGLCapability.POLYGON_OFFSET_FILL);
                    GraphicsExtensions.CheckGLError();
                    GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias * depthMul);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    GL.Disable(WebGLCapability.POLYGON_OFFSET_FILL);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastRasterizerState.DepthBias = this.DepthBias;
                device._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
            }

            if (device.Capabilities.SupportsDepthClamp &&
                (force ||
                 this.DepthClipEnable != device._lastRasterizerState.DepthClipEnable))
            {
                throw new PlatformNotSupportedException();
                device._lastRasterizerState.DepthClipEnable = this.DepthClipEnable;
            }

            // TODO: Implement MultiSampleAntiAlias
        }
    }
}
