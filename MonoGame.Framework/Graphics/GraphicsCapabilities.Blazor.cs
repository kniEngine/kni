// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {

        internal void PlatformInitialize(GraphicsDevice device)
        {
            var GL = device._glContext;

            _maxTextureSize = 2048;
            if (device.GraphicsProfile == GraphicsProfile.HiDef)
                _maxTextureSize = 4096;
            if (device.GraphicsProfile == GraphicsProfile.FL10_0)
                _maxTextureSize = 8192;
            if (device.GraphicsProfile == GraphicsProfile.FL10_1)
                _maxTextureSize = 8192;
            if (device.GraphicsProfile == GraphicsProfile.FL11_0)
                _maxTextureSize = 16384;
            if (device.GraphicsProfile == GraphicsProfile.FL11_1)
                _maxTextureSize = 16384;

            SupportsNonPowerOfTwo = device.GraphicsProfile >= GraphicsProfile.HiDef;

            SupportsTextureFilterAnisotropic = false; // TODO: check for TEXTURE_MAX_ANISOTROPY_EXT

            SupportsDepth24 = false;
            SupportsPackedDepthStencil = false;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = false;

            // Texture compression
            SupportsS3tc = GL.GetExtension("WEBGL_compressed_texture_s3tc");
            SupportsDxt1 = GL.GetExtension("WEBGL_compressed_texture_s3tc");

            SupportsSRgb = true;

            SupportsTextureArrays = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsDepthClamp = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsHalfFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsNormalized = device.GraphicsProfile >= GraphicsProfile.HiDef;
            
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

            _maxVertexBufferSlots = (device.GraphicsProfile >= GraphicsProfile.FL10_1) ? 32 : 16;

            SupportsInstancing = false;
            //TNC: TODO: detect suport based on feture level
            SupportsBaseIndexInstancing = false;
            SupportsSeparateBlendStates = true;

            MaxTextureAnisotropy = (device.GraphicsProfile == GraphicsProfile.Reach) ? 2 : 16;

            _maxMultiSampleCount = GetMaxMultiSampleCount(device);
        }

        private int GetMaxMultiSampleCount(GraphicsDevice device)
        {
            return 0;
        }
    }
}
