// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal WebGLTexture glTexture;
        internal WebGLTextureTarget glTarget;
        internal WebGLInternalFormat glInternalFormat;
        internal WebGLFormat glFormat;
        internal WebGLTexelType glType;
        internal bool _glIsCompressedTexture;
        internal SamplerState glLastSamplerState;

        private void PlatformGraphicsDeviceResetting()
        {
            if (glTexture != null)
                glTexture.Dispose();
            glTexture = null;
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (glTexture != null)
                    glTexture.Dispose();
                glTexture = null;
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        internal static void GetGLFormat(SurfaceFormat format,
                GraphicsDevice graphicsDevice,
                out WebGLInternalFormat glInternalFormat,
                out WebGLFormat glFormat,
                out WebGLTexelType glType,
                out bool glIsCompressedTexture)
        {
            var supportsS3tc = graphicsDevice.GraphicsCapabilities.SupportsS3tc;
            //var isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice.glMajorVersion == 2;

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
                
                default:
                    throw new PlatformNotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
            }
        }
    }

}

