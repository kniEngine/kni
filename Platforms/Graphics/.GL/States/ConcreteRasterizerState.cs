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
    internal class ConcreteRasterizerState : ResourceRasterizerStateStrategy
    {

        internal ConcreteRasterizerState(GraphicsContextStrategy contextStrategy, IRasterizerStateStrategy source)
            : base(contextStrategy, source)
        {
        }

        internal void PlatformApplyState(ConcreteGraphicsContextGL context, bool force = false)
        {
            var GL = context.GL;

            if (force)
            {
                // Turn off dithering to make sure data returned by Texture.GetData is accurate
                GL.Disable(EnableCap.Dither);
            }

            // When rendering offscreen the faces change order.
            bool offscreen = context.FramebufferRequireFlippedY;

            switch (CullMode)
            {
                case CullMode.None:
                    GL.Disable(EnableCap.CullFace);
                    GL.CheckGLError();
                    break;

                case CullMode.CullClockwiseFace:
                    GL.Enable(EnableCap.CullFace);
                    GL.CheckGLError();
                    GL.CullFace(CullFaceMode.Back);
                    GL.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Cw);
                    else
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    GL.CheckGLError();
                    break;

                case CullMode.CullCounterClockwiseFace:
                    GL.Enable(EnableCap.CullFace);
                    GL.CheckGLError();
                    GL.CullFace(CullFaceMode.Back);
                    GL.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    else
                        GL.FrontFace(FrontFaceDirection.Cw);
                    GL.CheckGLError();
                    break;

                default:
                    throw new InvalidOperationException("CullMode");
            }

#if DESKTOPGL
            if (FillMode == FillMode.WireFrame)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else // FillMode.Solid
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
#else
            if (FillMode == FillMode.WireFrame)
                throw new NotImplementedException();
#endif

            if (force ||
                this.ScissorTestEnable != context._lastRasterizerState.ScissorTestEnable)
			{
			    if (ScissorTestEnable)
				    GL.Enable(EnableCap.ScissorTest);
			    else
				    GL.Disable(EnableCap.ScissorTest);
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
                    DepthFormat activeDepthFormat;
                    if (context.IsRenderTargetBound)
                        activeDepthFormat = ((IRenderTarget)context.CurrentRenderTargetBindings[0].RenderTarget).DepthStencilFormat;
                    else
                        activeDepthFormat = base.GraphicsDeviceStrategy.PresentationParameters.DepthStencilFormat;

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
                    GL.Enable(EnableCap.PolygonOffsetFill);
                    GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias * depthMul);
                    GL.CheckGLError();
                }
                else
                {
                    GL.Disable(EnableCap.PolygonOffsetFill);
                    GL.CheckGLError();
                }
                context._lastRasterizerState.DepthBias = this.DepthBias;
                context._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
            }

            if (base.GraphicsDeviceStrategy.Capabilities.SupportsDepthClamp &&
                (force ||
                 this.DepthClipEnable != context._lastRasterizerState.DepthClipEnable))
            {
                if (!DepthClipEnable)
                    GL.Enable(EnableCap.DepthClamp);
                else
                    GL.Disable(EnableCap.DepthClamp);
                GL.CheckGLError();
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
