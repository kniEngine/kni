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
    internal class ConcreteRasterizerState : ResourceRasterizerStateStrategy
    {

        internal ConcreteRasterizerState(GraphicsContextStrategy contextStrategy, IRasterizerStateStrategy source)
            : base(contextStrategy, source)
        {
        }

        internal void PlatformApplyState(ConcreteGraphicsContext context, bool force = false)
        {
            var GL = context.GL;

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
                    GL.CheckGLError();
                    break;

                case CullMode.CullClockwiseFace:
                    GL.Enable(WebGLCapability.CULL_FACE);
                    GL.CheckGLError();
                    GL.CullFace(WebGLCullFaceMode.BACK);
                    GL.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(WebGLWinding.CW);
                    else
                        GL.FrontFace(WebGLWinding.CCW);
                    GL.CheckGLError();
                    break;

                case CullMode.CullCounterClockwiseFace:
                    GL.Enable(WebGLCapability.CULL_FACE);
                    GL.CheckGLError();
                    GL.CullFace(WebGLCullFaceMode.BACK);
                    GL.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(WebGLWinding.CCW);
                    else
                        GL.FrontFace(WebGLWinding.CW);
                    GL.CheckGLError();
                    break;

                default:
                    throw new InvalidOperationException("CullMode");
            }

            if (FillMode == FillMode.WireFrame)
                throw new PlatformNotSupportedException();

            if (force ||
                this.ScissorTestEnable != context._lastRasterizerState.ScissorTestEnable)
			{
			    if (ScissorTestEnable)
				    GL.Enable(WebGLCapability.SCISSOR_TEST);
			    else
				    GL.Disable(WebGLCapability.SCISSOR_TEST);
                GL.CheckGLError();
                context._lastRasterizerState.ScissorTestEnable = this.ScissorTestEnable;
            }

            if (force || 
                this.DepthBias != context._lastRasterizerState.DepthBias ||
                this.SlopeScaleDepthBias != context._lastRasterizerState.SlopeScaleDepthBias)
            {
                if (this.DepthBias != 0 || this.SlopeScaleDepthBias != 0)
                {
                    // from the docs it seems this works the same as for Direct3D
                    // https://www.khronos.org/opengles/sdk/docs/man/xhtml/glPolygonOffset.xml
                    // explanation for Direct3D is  in https://github.com/MonoGame/MonoGame/issues/4826
                    DepthFormat activeDepthFormat = (context.IsRenderTargetBound)
                                                  ? context._currentRenderTargetBindings[0].DepthFormat
                                                  : base.GraphicsDeviceStrategy.PresentationParameters.DepthStencilFormat;
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
                    GL.CheckGLError();
                    GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias * depthMul);
                    GL.CheckGLError();
                }
                else
                {
                    GL.Disable(WebGLCapability.POLYGON_OFFSET_FILL);
                    GL.CheckGLError();
                }
                context._lastRasterizerState.DepthBias = this.DepthBias;
                context._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
            }

            if (base.GraphicsDeviceStrategy.Capabilities.SupportsDepthClamp &&
                (force ||
                 this.DepthClipEnable != context._lastRasterizerState.DepthClipEnable))
            {
                throw new PlatformNotSupportedException();
                context._lastRasterizerState.DepthClipEnable = this.DepthClipEnable;
            }

            // TODO: Implement MultiSampleAntiAlias
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
