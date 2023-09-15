// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {

        internal void PlatformInitialize(GraphicsDeviceStrategy deviceStrategy)
        {
            IWebGLRenderingContext GL = deviceStrategy.MainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            GraphicsProfile profile = deviceStrategy.GraphicsProfile;

            _maxTextureSize = 2048;
            if (profile == GraphicsProfile.HiDef)
                _maxTextureSize = 4096;
            if (profile == GraphicsProfile.FL10_0)
                _maxTextureSize = 8192;
            if (profile == GraphicsProfile.FL10_1)
                _maxTextureSize = 8192;
            if (profile == GraphicsProfile.FL11_0)
                _maxTextureSize = 16384;
            if (profile == GraphicsProfile.FL11_1)
                _maxTextureSize = 16384;

            SupportsNonPowerOfTwo = profile >= GraphicsProfile.HiDef;

            SupportsTextureFilterAnisotropic = false; // TODO: check for TEXTURE_MAX_ANISOTROPY_EXT

            SupportsDepth24 = false;
            SupportsPackedDepthStencil = false;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = false;

            // Texture compression
            SupportsS3tc = GL.GetExtension("WEBGL_compressed_texture_s3tc");
            SupportsDxt1 = GL.GetExtension("WEBGL_compressed_texture_s3tc");

            SupportsSRgb = true;

            SupportsTextureArrays = profile >= GraphicsProfile.HiDef;
            SupportsDepthClamp = profile >= GraphicsProfile.HiDef;
            SupportsFloatTextures = profile >= GraphicsProfile.HiDef;
            SupportsHalfFloatTextures = profile >= GraphicsProfile.HiDef;
            SupportsNormalized = profile >= GraphicsProfile.HiDef;
            
            _maxTextureSlots = 8;
            _maxVertexTextureSlots = 0;
            // fix for bad GL drivers
            int maxCombinedTextureImageUnits;
            maxCombinedTextureImageUnits = 8;
            _maxTextureSlots = Math.Min(_maxTextureSlots, maxCombinedTextureImageUnits);
            _maxVertexTextureSlots = Math.Min(_maxVertexTextureSlots, maxCombinedTextureImageUnits);
            // limit texture slots to Reach profile limit until we implement profile detection.
            _maxTextureSlots = Math.Min(_maxTextureSlots, 16);
            _maxVertexTextureSlots = Math.Min(_maxTextureSlots, 0); // disable vertex textures until we implement it in WebGL.

            _maxVertexBufferSlots = (profile >= GraphicsProfile.FL10_1) ? 32 : 16;

            SupportsInstancing = false;
            //TNC: TODO: detect suport based on feture level
            SupportsBaseIndexInstancing = false;
            SupportsSeparateBlendStates = true;

            MaxTextureAnisotropy = (profile == GraphicsProfile.Reach) ? 2 : 16;
        }
    }
}
