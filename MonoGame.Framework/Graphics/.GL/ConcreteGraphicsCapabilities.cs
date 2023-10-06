// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using GetParamName = Microsoft.Xna.Platform.Graphics.OpenGL.GetPName;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsCapabilities : GraphicsCapabilities
    {
        /// <summary>
        /// True, if GL_ARB_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectARB { get; private set; }

        /// <summary>
        /// True, if GL_EXT_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectEXT { get; private set; }

        /// <summary>
        /// True, if GL_IMG_multisampled_render_to_texture is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectIMG { get; private set; }

#if DESKTOPGL
        private int _maxDrawBuffers;

        internal int MaxDrawBuffers { get { return _maxDrawBuffers; } }
#endif


        internal void PlatformInitialize(GraphicsDeviceStrategy deviceStrategy, int majorVersion, int minorVersion)
        {
            GraphicsProfile profile = deviceStrategy.GraphicsProfile;

            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

#if GLES
            SupportsNonPowerOfTwo = GL.Extensions.Contains("GL_OES_texture_npot") ||
                GL.Extensions.Contains("GL_ARB_texture_non_power_of_two") ||
                GL.Extensions.Contains("GL_IMG_texture_npot") ||
                GL.Extensions.Contains("GL_NV_texture_npot_2D_mipmap");
#elif DESKTOPGL
            // Unfortunately non PoT texture support is patchy even on desktop systems and we can't
            // rely on the fact that GL2.0+ supposedly supports npot in the core.
            // Reference: http://aras-p.info/blog/2012/10/17/non-power-of-two-textures/
            SupportsNonPowerOfTwo = _maxTextureSize >= 8192;
#endif

            SupportsTextureFilterAnisotropic = GL.Extensions.Contains("GL_EXT_texture_filter_anisotropic");

#if GLES
			SupportsDepth24 = GL.Extensions.Contains("GL_OES_depth24");
			SupportsPackedDepthStencil = GL.Extensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepthNonLinear = GL.Extensions.Contains("GL_NV_depth_nonlinear");
            SupportsTextureMaxLevel = GL.Extensions.Contains("GL_APPLE_texture_max_level");
#elif DESKTOPGL
            SupportsDepth24 = true;
            SupportsPackedDepthStencil = true;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;
#endif
            // Texture compression
            SupportsS3tc = GL.Extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                           GL.Extensions.Contains("GL_OES_texture_compression_S3TC") ||
                           GL.Extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                           GL.Extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || GL.Extensions.Contains("GL_EXT_texture_compression_dxt1");
            SupportsPvrtc = GL.Extensions.Contains("GL_IMG_texture_compression_pvrtc");
            SupportsEtc1 = GL.Extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
            SupportsAtitc = GL.Extensions.Contains("GL_ATI_texture_compression_atitc") ||
                            GL.Extensions.Contains("GL_AMD_compressed_ATC_texture");

            if (GL.BoundApi == GL.RenderApi.ES)
            {
                SupportsEtc2 = majorVersion >= 3;
            }


            // Framebuffer objects
#if GLES
            SupportsFramebufferObjectARB = GL.BoundApi == GL.RenderApi.ES && (majorVersion >= 2 || GL.Extensions.Contains("GL_ARB_framebuffer_object")); // always supported on GLES 2.0+
            SupportsFramebufferObjectEXT = GL.Extensions.Contains("GL_EXT_framebuffer_object");;
            SupportsFramebufferObjectIMG = GL.Extensions.Contains("GL_IMG_multisampled_render_to_texture") |
                                                 GL.Extensions.Contains("GL_APPLE_framebuffer_multisample") |
                                                 GL.Extensions.Contains("GL_EXT_multisampled_render_to_texture") |
                                                 GL.Extensions.Contains("GL_NV_framebuffer_multisample");
#elif DESKTOPGL
            // if we're on GL 3.0+, frame buffer extensions are guaranteed to be present, but extensions may be missing
            // it is then safe to assume that GL_ARB_framebuffer_object is present so that the standard function are loaded
            SupportsFramebufferObjectARB = majorVersion >= 3 || GL.Extensions.Contains("GL_ARB_framebuffer_object");
            SupportsFramebufferObjectEXT = GL.Extensions.Contains("GL_EXT_framebuffer_object");
#endif
            // Anisotropic filtering
            int anisotropy = 0;
            if (SupportsTextureFilterAnisotropic)
            {
                GL.GetInteger((GetPName)GetParamName.MaxTextureMaxAnisotropyExt, out anisotropy);
                GraphicsExtensions.CheckGLError();
            }
            MaxTextureAnisotropy = anisotropy;

            // sRGB
#if GLES
            SupportsSRgb = GL.Extensions.Contains("GL_EXT_sRGB");
            SupportsFloatTextures = GL.BoundApi == GL.RenderApi.ES && (majorVersion >= 3 || GL.Extensions.Contains("GL_EXT_color_buffer_float"));
            SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.ES && (majorVersion >= 3 || GL.Extensions.Contains("GL_EXT_color_buffer_half_float"));
            SupportsNormalized = GL.BoundApi == GL.RenderApi.ES && (majorVersion >= 3 && GL.Extensions.Contains("GL_EXT_texture_norm16"));
#elif DESKTOPGL
            SupportsSRgb = GL.Extensions.Contains("GL_EXT_texture_sRGB") && GL.Extensions.Contains("GL_EXT_framebuffer_sRGB");
            SupportsFloatTextures = GL.BoundApi == GL.RenderApi.GL && (majorVersion >= 3 || GL.Extensions.Contains("GL_ARB_texture_float"));
            SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.GL && (majorVersion >= 3 || GL.Extensions.Contains("GL_ARB_half_float_pixel"));;
            SupportsNormalized = GL.BoundApi == GL.RenderApi.GL && (majorVersion >= 3 || GL.Extensions.Contains("GL_EXT_texture_norm16"));;
#endif

            // TODO: Implement OpenGL support for texture arrays
            // once we can author shaders that use texture arrays.
            SupportsTextureArrays = false;

            SupportsDepthClamp = GL.Extensions.Contains("GL_ARB_depth_clamp");

            GL.GetInteger(GetPName.MaxTextureImageUnits, out _maxTextureSlots);
            GraphicsExtensions.CheckGLError();
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits, out _maxVertexTextureSlots);
            GraphicsExtensions.CheckGLError();
            // fix for bad GL drivers
            int maxCombinedTextureImageUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out maxCombinedTextureImageUnits);
            _maxTextureSlots = Math.Min(_maxTextureSlots, maxCombinedTextureImageUnits);
            _maxVertexTextureSlots = Math.Min(_maxVertexTextureSlots, maxCombinedTextureImageUnits);
            // limit texture slots to Reach profile limit until we implement profile detection.
            _maxTextureSlots = Math.Min(_maxTextureSlots, 16);
            _maxVertexTextureSlots = Math.Min(_maxTextureSlots, 0); // disable vertex textures until we implement it in WebGL.

            int maxVertexAttribs;
            GL.GetInteger(GetPName.MaxVertexAttribs, out maxVertexAttribs);
            GraphicsExtensions.CheckGLError();
            _maxVertexBufferSlots = (profile >= GraphicsProfile.FL10_1) ? 32 : 16;
            _maxVertexBufferSlots = Math.Min(_maxVertexBufferSlots, maxVertexAttribs);

            SupportsInstancing = GL.VertexAttribDivisor != null;

            SupportsBaseIndexInstancing = GL.DrawElementsInstancedBaseInstance != null;

#if GLES
            SupportsSeparateBlendStates = false;
#elif DESKTOPGL
            SupportsSeparateBlendStates = majorVersion >= 4 || GL.Extensions.Contains("GL_ARB_draw_buffers_blend");
#endif

#if DESKTOPGL
            GL.GetInteger(GetPName.MaxDrawBuffers, out _maxDrawBuffers);
            GraphicsExtensions.CheckGLError();
#endif
        }

    }
}
