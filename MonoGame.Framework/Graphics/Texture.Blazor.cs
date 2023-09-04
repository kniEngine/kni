// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {

        private void PlatformGraphicsDeviceResetting()
        {
            if (GetTextureStrategy<ConcreteTexture>()._glTexture != null)
                GetTextureStrategy<ConcreteTexture>()._glTexture.Dispose();
            GetTextureStrategy<ConcreteTexture>()._glTexture = null;
            GetTextureStrategy<ConcreteTexture>()._glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (GetTextureStrategy<ConcreteTexture>()._glTexture != null)
                    GetTextureStrategy<ConcreteTexture>()._glTexture.Dispose();
                GetTextureStrategy<ConcreteTexture>()._glTexture = null;
                GetTextureStrategy<ConcreteTexture>()._glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        internal static void ToGLSurfaceFormat(SurfaceFormat format,
                GraphicsDevice graphicsDevice,
                out WebGLInternalFormat glInternalFormat,
                out WebGLFormat glFormat,
                out WebGLTexelType glType,
                out bool glIsCompressedTexture)
        {
            var supportsS3tc = graphicsDevice.Strategy.Capabilities.SupportsS3tc;
            //var isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice._glMajorVersion == 2;

            switch (format)
            {
                case SurfaceFormat.Bgr565:
                    glInternalFormat        = WebGLInternalFormat.RGB;
                    glFormat                = WebGLFormat.RGB;
                    glType                  = WebGLTexelType.UNSIGNED_SHORT_5_6_5;
                    glIsCompressedTexture   = false;
                    break;
                case SurfaceFormat.Color:
                    glInternalFormat        = WebGLInternalFormat.RGBA;
                    glFormat                = WebGLFormat.RGBA;
                    glType                  = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture   = false;
                    break;
                case SurfaceFormat.Bgra4444:
				    glInternalFormat        = WebGLInternalFormat.RGBA;
                    glFormat                = WebGLFormat.RGBA;
                    glType                  = WebGLTexelType.UNSIGNED_SHORT_4_4_4_4;
                    glIsCompressedTexture   = false;
                    break;
                case SurfaceFormat.Bgra5551:
                    glInternalFormat        = WebGLInternalFormat.RGBA;
                    glFormat                = WebGLFormat.RGBA;
                    glType                  = WebGLTexelType.UNSIGNED_SHORT_5_5_5_1;
                    glIsCompressedTexture   = false;
                    break;
                case SurfaceFormat.Alpha8:
                    glInternalFormat        = WebGLInternalFormat.LUMINANCE;
                    glFormat                = WebGLFormat.LUMINANCE;
                    glType                  = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture   = false;
                    break;

                case SurfaceFormat.Dxt1:
                    if (!supportsS3tc) goto default;
                    glInternalFormat = WebGLInternalFormat.COMPRESSED_RGB_S3TC_DXT1_EXT;
                    glFormat = (WebGLFormat)0x83F0;
                    glType = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = true;
                    break;
                case SurfaceFormat.Dxt3:
                    if (!supportsS3tc) goto default;
                    glInternalFormat = WebGLInternalFormat.COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    glFormat = (WebGLFormat)0x83F2;
                    glType = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = true;
                    break;
                
                default:
                    throw new PlatformNotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
            }
        }

        protected void PlatformCreateRenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            var GL = graphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

            if (preferredMultiSampleCount > 0 && graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>()._supportsBlitFramebuffer)
            {
                throw new NotImplementedException();
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                WebGLRenderbufferInternalFormat depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                WebGLRenderbufferInternalFormat stencilInternalFormat = (WebGLRenderbufferInternalFormat)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.None:
                        break;

                    case DepthFormat.Depth16:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;

                    case DepthFormat.Depth24:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;

                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        stencilInternalFormat = WebGLRenderbufferInternalFormat.STENCIL_INDEX8;
                        break;

                    default:
                        throw new InvalidOperationException("preferredDepthFormat");
                }

                if (depthInternalFormat != 0)
                {
                    depth = GL.CreateRenderbuffer();
                    GraphicsExtensions.CheckGLError();
                    GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, depth);
                    GraphicsExtensions.CheckGLError();
                    if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                        throw new NotImplementedException();
                    else
                        GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                    GraphicsExtensions.CheckGLError();
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            stencil = GL.CreateRenderbuffer();
                            GraphicsExtensions.CheckGLError();
                            GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, stencil);
                            GraphicsExtensions.CheckGLError();
                            if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                                throw new NotImplementedException();
                            else
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, stencilInternalFormat, width, height);
                            GraphicsExtensions.CheckGLError();
                        }
                    }
                }
            }

            IRenderTargetGL renderTargetGL = (IRenderTargetGL)this;
            if (color != null)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        protected void PlatformDeleteRenderTarget()
        {
            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

            IRenderTargetGL renderTargetGL = (IRenderTargetGL)this;
            color = renderTargetGL.GLColorBuffer;
            depth = renderTargetGL.GLDepthBuffer;
            stencil = renderTargetGL.GLStencilBuffer;
            bool colorIsRenderbuffer = renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture;

            if (color != null)
            {
                if (colorIsRenderbuffer)
                {
                    throw new NotImplementedException();
                }
                if (stencil != null && stencil != depth)
                {
                    stencil.Dispose();
                    GraphicsExtensions.CheckGLError();
                }
                if (depth != null)
                {
                    depth.Dispose();
                    GraphicsExtensions.CheckGLError();
                }

                GraphicsDevice.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().PlatformUnbindRenderTarget((IRenderTarget)this);
            }
        }

    }

}

