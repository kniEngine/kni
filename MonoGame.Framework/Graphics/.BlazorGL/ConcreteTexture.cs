// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture : GraphicsResourceStrategy, ITextureStrategy
    {
        private readonly GraphicsContextStrategy _contextStrategy;
        private readonly SurfaceFormat _format;        
        private readonly int _levelCount;

        internal ConcreteTexture(GraphicsContextStrategy contextStrategy, SurfaceFormat format, int levelCount)
            : base(contextStrategy)
        {
            this._contextStrategy = contextStrategy;
            this._format = format;
            this._levelCount = levelCount;
        }


        #region ITextureStrategy
        public SurfaceFormat Format { get { return _format; } }
        public int LevelCount { get { return _levelCount; } }
        #endregion ITextureStrategy


        internal WebGLTexture _glTexture;
        internal WebGLTextureTarget _glTarget;
        internal WebGLInternalFormat _glInternalFormat;
        internal WebGLFormat _glFormat;
        internal WebGLTexelType _glType;
        internal bool _glIsCompressedTexture;
        internal SamplerState _glLastSamplerState;

        internal static void ToGLSurfaceFormat(SurfaceFormat format,
                GraphicsContextStrategy contextStrategy,
                out WebGLInternalFormat glInternalFormat,
                out WebGLFormat glFormat,
                out WebGLTexelType glType,
                out bool glIsCompressedTexture)
        {
            bool supportsS3tc = contextStrategy.Context.DeviceStrategy.Capabilities.SupportsS3tc;
            //bool isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice._glMajorVersion == 2;

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

        internal static void PlatformCreateRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

            if (multiSampleCount > 0)
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
                    GL.CheckGLError();
                    GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, depth);
                    GL.CheckGLError();
                    if (multiSampleCount > 0)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                        GL.CheckGLError();
                    }
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            stencil = GL.CreateRenderbuffer();
                            GL.CheckGLError();
                            GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, stencil);
                            GL.CheckGLError();
                            if (multiSampleCount > 0)
                            {
                                /* System.Diagnostics.Debug.Assert(GL.RenderbufferStorageMultisample != null); */
                                throw new NotImplementedException();
                            }
                            else
                            {
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, stencilInternalFormat, width, height);
                                GL.CheckGLError();
                            }
                        }
                    }
                }
            }

            if (color != null)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        internal static void PlatformDeleteRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsContextStrategy contextStrategy)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

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
                    GL.CheckGLError();
                }
                if (depth != null)
                {
                    depth.Dispose();
                    GL.CheckGLError();
                }

                contextStrategy.ToConcrete<ConcreteGraphicsContext>().PlatformUnbindRenderTarget(renderTargetGL);
            }
        }


        internal override void PlatformGraphicsContextLost()
        {
            if (_glTexture != null)
            {
                _glTexture.Dispose();
            }
            _glTexture = null;
            _glLastSamplerState = null;

            base.PlatformGraphicsContextLost();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_glTexture != null)
                    _glTexture.Dispose();
                _glTexture = null;
                _glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }
    }
}
