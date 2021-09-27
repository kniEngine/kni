// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {


        private void PlatformInitialize(GraphicsDevice device)
        {
            var GL = device._glContext;

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
            SupportsVertexTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsHalfFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsNormalized = device.GraphicsProfile >= GraphicsProfile.HiDef;

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
