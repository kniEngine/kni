// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal int glTexture = -1;
        internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal PixelInternalFormat glInternalFormat;
        internal PixelFormat glFormat;
        internal PixelType glType;
        internal SamplerState glLastSamplerState;

        private void PlatformGraphicsDeviceResetting()
        {
            DeleteGLTexture();
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        private void DeleteGLTexture()
        {
            if (glTexture > 0)
            {
                GraphicsDevice.DisposeTexture(glTexture);
            }
            glTexture = -1;
        }

        const SurfaceFormat InvalidFormat = (SurfaceFormat)int.MaxValue;
        internal static void ToGLSurfaceFormat(SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out PixelInternalFormat glInternalFormat,
            out PixelFormat glFormat,
            out PixelType glType)
        {
            glInternalFormat = PixelInternalFormat.Rgba;
            glFormat = PixelFormat.Rgba;
            glType = PixelType.UnsignedByte;

            var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
            var supportsS3tc = graphicsDevice.GraphicsCapabilities.SupportsS3tc;
            var supportsPvrtc = graphicsDevice.GraphicsCapabilities.SupportsPvrtc;
            var supportsEtc1 = graphicsDevice.GraphicsCapabilities.SupportsEtc1;
            var supportsEtc2 = graphicsDevice.GraphicsCapabilities.SupportsEtc2;
            var supportsAtitc = graphicsDevice.GraphicsCapabilities.SupportsAtitc;
            var supportsFloat = graphicsDevice.GraphicsCapabilities.SupportsFloatTextures;
            var supportsHalfFloat = graphicsDevice.GraphicsCapabilities.SupportsHalfFloatTextures;
            var supportsNormalized = graphicsDevice.GraphicsCapabilities.SupportsNormalized;
            var isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice.glMajorVersion == 2;

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
    }
}

