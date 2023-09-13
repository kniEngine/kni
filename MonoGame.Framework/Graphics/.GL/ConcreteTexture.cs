// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


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



        internal int _glTexture = -1;
        internal TextureTarget _glTarget;
        internal TextureUnit _glTextureUnit = TextureUnit.Texture0;
        internal PixelInternalFormat _glInternalFormat;
        internal PixelFormat _glFormat;
        internal PixelType _glType;
        internal SamplerState _glLastSamplerState;


        const SurfaceFormat InvalidFormat = (SurfaceFormat)int.MaxValue;
        internal static void ToGLSurfaceFormat(SurfaceFormat format,
            GraphicsDeviceStrategy deviceStrategy,
            out PixelInternalFormat glInternalFormat,
            out PixelFormat glFormat,
            out PixelType glType)
        {
            glInternalFormat = PixelInternalFormat.Rgba;
            glFormat = PixelFormat.Rgba;
            glType = PixelType.UnsignedByte;

            bool supportsSRgb = deviceStrategy.Capabilities.SupportsSRgb;
            bool supportsS3tc = deviceStrategy.Capabilities.SupportsS3tc;
            bool supportsPvrtc = deviceStrategy.Capabilities.SupportsPvrtc;
            bool supportsEtc1 = deviceStrategy.Capabilities.SupportsEtc1;
            bool supportsEtc2 = deviceStrategy.Capabilities.SupportsEtc2;
            bool supportsAtitc = deviceStrategy.Capabilities.SupportsAtitc;
            bool supportsFloat = deviceStrategy.Capabilities.SupportsFloatTextures;
            bool supportsHalfFloat = deviceStrategy.Capabilities.SupportsHalfFloatTextures;
            bool supportsNormalized = deviceStrategy.Capabilities.SupportsNormalized;
            bool isGLES2 = GL.BoundApi == GL.RenderApi.ES && deviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._glMajorVersion == 2;

            switch (format)
            {
                case SurfaceFormat.Color:
                    glInternalFormat = PixelInternalFormat.Rgba;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.ColorSRgb:
                    if (!supportsSRgb)
                        goto case SurfaceFormat.Color;
                    glInternalFormat = PixelInternalFormat.Srgb;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Bgr565:
                    glInternalFormat = PixelInternalFormat.Rgb;
                    glFormat = PixelFormat.Rgb;
                    glType = PixelType.UnsignedShort565;
                    break;
                case SurfaceFormat.Bgra4444:
#if IOS || TVOS || ANDROID
				glInternalFormat = PixelInternalFormat.Rgba;
#else
                    glInternalFormat = PixelInternalFormat.Rgba4;
#endif
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedShort4444;
                    break;
                case SurfaceFormat.Bgra5551:
                    glInternalFormat = PixelInternalFormat.Rgba;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedShort5551;
                    break;
                case SurfaceFormat.Alpha8:
                    glInternalFormat = PixelInternalFormat.Luminance;
                    glFormat = PixelFormat.Luminance;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Dxt1:
                    if (!supportsS3tc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    if (!supportsSRgb)
                        goto case SurfaceFormat.Dxt1;
                    glInternalFormat = PixelInternalFormat.CompressedSrgbS3tcDxt1Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt1a:
                    if (!supportsS3tc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt3:
                    if (!supportsS3tc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                    if (!supportsSRgb)
                        goto case SurfaceFormat.Dxt3;
                    glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt5:
                    if (!supportsS3tc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Dxt5SRgb:
                    if (!supportsSRgb)
                        goto case SurfaceFormat.Dxt5;
                    glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
#if !IOS && !TVOS && !ANDROID
                case SurfaceFormat.Rgba1010102:
                    glInternalFormat = PixelInternalFormat.Rgb10A2ui;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedInt1010102;
                    break;
#endif
                case SurfaceFormat.Single:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.R32f;
                    glFormat = PixelFormat.Red;
                    glType = PixelType.Float;
                    break;

                case SurfaceFormat.HalfVector2:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg16f;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.HalfFloat;
                    break;

                // HdrBlendable implemented as HalfVector4 (see http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/surfaceformat-hdrblendable.aspx)
                case SurfaceFormat.HdrBlendable:
                case SurfaceFormat.HalfVector4:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgba16f;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.HalfFloat;
                    break;

                case SurfaceFormat.HalfSingle:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.R16f;
                    glFormat = PixelFormat.Red;
                    glType = isGLES2 ? PixelType.HalfFloatOES : PixelType.HalfFloat;
                    break;

                case SurfaceFormat.Vector2:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg32f;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.Float;
                    break;

                case SurfaceFormat.Vector4:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgba32f;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.Float;
                    break;

                case SurfaceFormat.NormalizedByte2:
                    glInternalFormat = PixelInternalFormat.Rg8i;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.Byte;
                    break;

                case SurfaceFormat.NormalizedByte4:
                    glInternalFormat = PixelInternalFormat.Rgba8i;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.Byte;
                    break;

                case SurfaceFormat.Rg32:
                    if (!supportsNormalized)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg16ui;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.UnsignedShort;
                    break;

                case SurfaceFormat.Rgba64:
                    if (!supportsNormalized)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgba16;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedShort;
                    break;
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                    if (!supportsAtitc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.AtcRgbaExplicitAlphaAmd;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                    if (!supportsAtitc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.AtcRgbaInterpolatedAlphaAmd;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbEtc1:
                    if (!supportsEtc1)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc1; // GL_ETC1_RGB8_OES
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Rgb8Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2Rgb8; // GL_COMPRESSED_RGB8_ETC2
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Srgb8Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2Srgb8; // GL_COMPRESSED_SRGB8_ETC2
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Rgb8A1Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2Rgb8A1; // GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Srgb8A1Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2Srgb8A1; // GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.Rgba8Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2Rgba8Eac; // GL_COMPRESSED_RGBA8_ETC2_EAC
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.SRgb8A8Etc2:
                    if (!supportsEtc2)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Etc2SRgb8A8Eac; // GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbPvrtc2Bpp:
                    if (!supportsPvrtc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbPvrtc4Bpp:
                    if (!supportsPvrtc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    if (!supportsPvrtc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    if (!supportsPvrtc)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img;
                    glFormat = PixelFormat.CompressedTextureFormats;
                    break;

                case InvalidFormat:
                default:
                    throw new NotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
            }
        }

        internal static void PlatformCreateRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsDeviceStrategy deviceStrategy, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            int color = 0;
            int depth = 0;
            int stencil = 0;

            if (preferredMultiSampleCount > 0 && deviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsBlitFramebuffer)
            {
                color = GL.GenRenderbuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, color);
                GraphicsExtensions.CheckGLError();
                if (preferredMultiSampleCount > 0 && GL.RenderbufferStorageMultisample != null)
                    GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, preferredMultiSampleCount, RenderbufferStorage.Rgba8, width, height);
                else
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
                GraphicsExtensions.CheckGLError();
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                RenderbufferStorage depthInternalFormat = RenderbufferStorage.DepthComponent16;
                RenderbufferStorage stencilInternalFormat = (RenderbufferStorage)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.None:
                        break;

                    case DepthFormat.Depth16:
                        depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;

#if GLES
                    case DepthFormat.Depth24:
                        if (deviceStrategy.Capabilities.SupportsDepth24)
                            depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                        else if (deviceStrategy.Capabilities.SupportsDepthNonLinear)
                            depthInternalFormat = RenderbufferStorage.DepthComponent16NonlinearNv;
                        else
                            depthInternalFormat = RenderbufferStorage.DepthComponent16;
                        break;

                    case DepthFormat.Depth24Stencil8:
                        if (deviceStrategy.Capabilities.SupportsPackedDepthStencil)
                            depthInternalFormat = RenderbufferStorage.Depth24Stencil8Oes;
                        else
                        {
                            if (deviceStrategy.Capabilities.SupportsDepth24)
                                depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                            else if (deviceStrategy.Capabilities.SupportsDepthNonLinear)
                                depthInternalFormat = RenderbufferStorage.DepthComponent16NonlinearNv;
                            else
                                depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            stencilInternalFormat = RenderbufferStorage.StencilIndex8;
                        }
                        break;

#else
                    case DepthFormat.Depth24:
                        depthInternalFormat = RenderbufferStorage.DepthComponent24;
                        break;

                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = RenderbufferStorage.Depth24Stencil8;
                        break;
#endif

                    default:
                        throw new InvalidOperationException("preferredDepthFormat");
                }

                if (depthInternalFormat != 0)
                {
                    depth = GL.GenRenderbuffer();
                    GraphicsExtensions.CheckGLError();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth);
                    GraphicsExtensions.CheckGLError();
                    if (preferredMultiSampleCount > 0 && GL.RenderbufferStorageMultisample != null)
                        GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, preferredMultiSampleCount, depthInternalFormat, width, height);
                    else
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depthInternalFormat, width, height);
                    GraphicsExtensions.CheckGLError();
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            stencil = GL.GenRenderbuffer();
                            GraphicsExtensions.CheckGLError();
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, stencil);
                            GraphicsExtensions.CheckGLError();
                            if (preferredMultiSampleCount > 0 && GL.RenderbufferStorageMultisample != null)
                                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, preferredMultiSampleCount, stencilInternalFormat, width, height);
                            else
                                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, stencilInternalFormat, width, height);
                            GraphicsExtensions.CheckGLError();
                        }
                    }
                }
            }

            if (color != 0)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        internal static void PlatformDeleteRenderTarget(IRenderTargetStrategyGL renderTargetGL, GraphicsDeviceStrategy deviceStrategy)
        {
            int color = 0;
            int depth = 0;
            int stencil = 0;

            color = renderTargetGL.GLColorBuffer;
            depth = renderTargetGL.GLDepthBuffer;
            stencil = renderTargetGL.GLStencilBuffer;
            bool colorIsRenderbuffer = renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture;

            if (color != 0)
            {
                if (colorIsRenderbuffer)
                {
                    GL.DeleteRenderbuffer(color);
                    GraphicsExtensions.CheckGLError();
                }
                if (stencil != 0 && stencil != depth)
                {
                    GL.DeleteRenderbuffer(stencil);
                    GraphicsExtensions.CheckGLError();
                }
                if (depth != 0)
                {
                    GL.DeleteRenderbuffer(depth);
                    GraphicsExtensions.CheckGLError();
                }

                deviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().PlatformUnbindRenderTarget((IRenderTarget)renderTargetGL);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            
            if (_glTexture > 0)
            {
                if (!GraphicsDevice.IsDisposed)
                {
                    GL.DeleteTexture(_glTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            _glTexture = -1;

            _glLastSamplerState = null;

            base.Dispose(disposing);
        }

    }
}
