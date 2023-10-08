// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        internal void PlatformApplyState(ConcreteGraphicsContextGL context, bool force = false)
        {
            var GL = OGL.Current;

            if (force)
            {
                // Turn off dithering to make sure data returned by Texture.GetData is accurate
                GL.Disable(EnableCap.Dither);
            }

            // When rendering offscreen the faces change order.
            bool offscreen = context.IsRenderTargetBound;

            switch (CullMode)
            {
                case CullMode.None:
                    GL.Disable(EnableCap.CullFace);
                    GraphicsExtensions.CheckGLError();
                    break;

                case Graphics.CullMode.CullClockwiseFace:
                    GL.Enable(EnableCap.CullFace);
                    GraphicsExtensions.CheckGLError();
                    GL.CullFace(CullFaceMode.Back);
                    GraphicsExtensions.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Cw);
                    else
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    GraphicsExtensions.CheckGLError();
                    break;

                case Graphics.CullMode.CullCounterClockwiseFace:
                    GL.Enable(EnableCap.CullFace);
                    GraphicsExtensions.CheckGLError();
                    GL.CullFace(CullFaceMode.Back);
                    GraphicsExtensions.CheckGLError();
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    else
                        GL.FrontFace(FrontFaceDirection.Cw);
                    GraphicsExtensions.CheckGLError();
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
                GraphicsExtensions.CheckGLError();
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
                                                  : this.GraphicsDevice.PresentationParameters.DepthStencilFormat;
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
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    GL.Disable(EnableCap.PolygonOffsetFill);
                    GraphicsExtensions.CheckGLError();
                }
                context._lastRasterizerState.DepthBias = this.DepthBias;
                context._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
            }

            if (this.GraphicsDevice.Strategy.Capabilities.SupportsDepthClamp &&
                (force ||
                 this.DepthClipEnable != context._lastRasterizerState.DepthClipEnable))
            {
                if (!DepthClipEnable)
                    GL.Enable(EnableCap.DepthClamp);
                else
                    GL.Disable(EnableCap.DepthClamp);
                GraphicsExtensions.CheckGLError();
                context._lastRasterizerState.DepthClipEnable = this.DepthClipEnable;
            }

            // TODO: Implement MultiSampleAntiAlias
        }
    }
}
