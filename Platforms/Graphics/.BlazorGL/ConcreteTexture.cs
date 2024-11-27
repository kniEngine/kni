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
            bool supportsS3tc = contextStrategy.Capabilities.SupportsS3tc;
            //bool isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice._glMajorVersion == 2;
            bool supportsFloat = contextStrategy.Capabilities.SupportsFloatTextures;

            switch (format)
            {
                case SurfaceFormat.Color:
                    glInternalFormat      = WebGLInternalFormat.RGBA;
                    glFormat              = WebGLFormat.RGBA;
                    glType                = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Bgr565:
                    glInternalFormat      = WebGLInternalFormat.RGB;
                    glFormat              = WebGLFormat.RGB;
                    glType                = WebGLTexelType.UNSIGNED_SHORT_5_6_5;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Bgra4444:
				    glInternalFormat      = WebGLInternalFormat.RGBA;
                    glFormat              = WebGLFormat.RGBA;
                    glType                = WebGLTexelType.UNSIGNED_SHORT_4_4_4_4;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Bgra5551:
                    glInternalFormat      = WebGLInternalFormat.RGBA;
                    glFormat              = WebGLFormat.RGBA;
                    glType                = WebGLTexelType.UNSIGNED_SHORT_5_5_5_1;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Alpha8:
                    glInternalFormat      = WebGLInternalFormat.LUMINANCE;
                    glFormat              = WebGLFormat.LUMINANCE;
                    glType                = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = false;
                    break;

                case SurfaceFormat.Dxt1:
                    if (!supportsS3tc) goto default;
                    glInternalFormat      = WebGLInternalFormat.COMPRESSED_RGB_S3TC_DXT1_EXT;
                    glFormat              = (WebGLFormat)glInternalFormat;
                    glType                = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = true;
                    break;
                case SurfaceFormat.Dxt3:
                    if (!supportsS3tc) goto default;
                    glInternalFormat      = WebGLInternalFormat.COMPRESSED_RGBA_S3TC_DXT3_EXT;
                    glFormat              = (WebGLFormat)glInternalFormat;
                    glType                = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = true;
                    break;
                case SurfaceFormat.Dxt5:
                    if (!supportsS3tc) goto default;
                    glInternalFormat      = WebGLInternalFormat.COMPRESSED_RGBA_S3TC_DXT5_EXT;
                    glFormat              = (WebGLFormat)glInternalFormat;
                    glType                = WebGLTexelType.UNSIGNED_BYTE;
                    glIsCompressedTexture = true;
                    break;

                // float formats
                case SurfaceFormat.Single:
                    if (!supportsFloat) goto default;
                    glInternalFormat      = WebGLInternalFormat.R32F;
                    glFormat              = WebGLFormat.RED;
                    glType                = WebGLTexelType.FLOAT;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Vector2:
                    if (!supportsFloat) goto default;
                    glInternalFormat      = WebGLInternalFormat.RG32F;
                    glFormat              = WebGLFormat.RG;
                    glType                = WebGLTexelType.FLOAT;
                    glIsCompressedTexture = false;
                    break;
                case SurfaceFormat.Vector4:
                    if (!supportsFloat) goto default;
                    glInternalFormat      = WebGLInternalFormat.RGBA32F;
                    glFormat              = WebGLFormat.RGBA;
                    glType                = WebGLTexelType.FLOAT;
                    glIsCompressedTexture = false;
                    break;

                default:
                    throw new PlatformNotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
            }
        }

        internal static void PlatformCreateRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            //contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();
            {
                var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                if (multiSampleCount > 0)
                {
                    WebGLRenderbufferInternalFormat colorInternalFormat = default;
                    switch (preferredFormat)
                    {
                        case SurfaceFormat.Color:
                            break;
                        case SurfaceFormat.Bgr565:
                            colorInternalFormat = WebGLRenderbufferInternalFormat.RGB565;
                            break;
                        case SurfaceFormat.Bgra4444:
                            colorInternalFormat = WebGLRenderbufferInternalFormat.RGBA4;
                            break;
                        case SurfaceFormat.Bgra5551:
                            colorInternalFormat = WebGLRenderbufferInternalFormat.RGB5_A1;
                            break;
                        case SurfaceFormat.Single:
                            break;
                        case SurfaceFormat.HalfSingle:
                            break;
                        case SurfaceFormat.Vector2:
                            break;
                        case SurfaceFormat.HalfVector2:
                            break;
                        case SurfaceFormat.Vector4:
                            break;
                        case SurfaceFormat.HalfVector4:
                            break;

                        default:
                            break;
                    }

                    renderTargetGL.GLColorBuffer = GL.CreateRenderbuffer();
                    GL.CheckGLError();
                    GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLColorBuffer);
                    GL.CheckGLError();

                    System.Diagnostics.Debug.Assert(GL is IWebGL2RenderingContext); // (GL.RenderbufferStorageMultisample == null)
                    throw new NotImplementedException();
                }

                if (preferredDepthFormat != DepthFormat.None)
                {
                    WebGLRenderbufferInternalFormat depthInternalFormat = default;
                    WebGLRenderbufferInternalFormat stencilInternalFormat = default;
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
                            depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_STENCIL;
                            break;

                        default:
                            throw new InvalidOperationException("preferredDepthFormat");
                    }

                    if (depthInternalFormat != 0)
                    {
                        renderTargetGL.GLDepthBuffer = GL.CreateRenderbuffer();
                        GL.CheckGLError();
                        GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLDepthBuffer);
                        GL.CheckGLError();
                        if (multiSampleCount > 0)
                        {
                            System.Diagnostics.Debug.Assert(GL is IWebGL2RenderingContext); // (GL.RenderbufferStorageMultisample == null)
                            throw new NotImplementedException();
                        }
                        else
                        {
                            if (GL is IWebGL2RenderingContext)
                            {
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                                GL.CheckGLError();
                            }
                            else
                            {
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                                GL.CheckGLError();
                            }
                        }

                        if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        {
                            renderTargetGL.GLStencilBuffer = renderTargetGL.GLDepthBuffer;
                        }
                    }
                }
            }
            return;
        }

        internal static void PlatformDeleteRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsContextStrategy contextStrategy)
        {
            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            {
                if (renderTargetGL.GLColorBuffer != null)
                {
                    renderTargetGL.GLColorBuffer.Dispose();
                    GL.CheckGLError();
                }
                if (renderTargetGL.GLStencilBuffer != null && renderTargetGL.GLStencilBuffer != renderTargetGL.GLDepthBuffer)
                {
                    renderTargetGL.GLStencilBuffer.Dispose();
                    GL.CheckGLError();
                }
                if (renderTargetGL.GLDepthBuffer != null)
                {
                    renderTargetGL.GLDepthBuffer.Dispose();
                    GL.CheckGLError();
                }

                contextStrategy.ToConcrete<ConcreteGraphicsContext>().PlatformUnbindRenderTarget(renderTargetGL);
            }
        }


        protected override void PlatformGraphicsContextLost()
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
