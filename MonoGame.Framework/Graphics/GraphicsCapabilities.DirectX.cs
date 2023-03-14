// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {
        private void PlatformInitialize(GraphicsDevice device)
        {
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
            SupportsTextureFilterAnisotropic = true;

            SupportsDepth24 = false;
            SupportsPackedDepthStencil = false;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = false;

            // Texture compression
            SupportsDxt1 = true;
            SupportsS3tc = true;

            SupportsSRgb = true;

            SupportsTextureArrays = device.GraphicsProfile >= GraphicsProfile.FL10_0;
            SupportsDepthClamp = device.GraphicsProfile >= GraphicsProfile.HiDef;

            _maxTextureSlots = 16;
            _maxVertexTextureSlots = (device.GraphicsProfile >= GraphicsProfile.FL10_0) ? 16 : 0;

            _maxVertexBufferSlots = (device.GraphicsProfile >= GraphicsProfile.FL10_1) ? 32 : 16;

            SupportsFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsHalfFloatTextures = device.GraphicsProfile >= GraphicsProfile.HiDef;
            SupportsNormalized = device.GraphicsProfile >= GraphicsProfile.HiDef;

            SupportsInstancing = true;
            //TNC: TODO: detect suport based on feture level
            SupportsBaseIndexInstancing = false;
            SupportsSeparateBlendStates = true;

            MaxTextureAnisotropy = (device.GraphicsProfile == GraphicsProfile.Reach) ? 2 : 16;

            _maxMultiSampleCount = GetMaxMultiSampleCount(device);
        }

        private int GetMaxMultiSampleCount(GraphicsDevice device)
        {
            var format = GraphicsExtensions.ToDXFormat(device.PresentationParameters.BackBufferFormat);
            // Find the maximum supported level starting with the game's requested multisampling level
            // and halving each time until reaching 0 (meaning no multisample support).
            var qualityLevels = 0;
            var maxLevel = MultiSampleCountLimit;
            while (maxLevel > 0)
            {
                qualityLevels = device.D3DDevice.CheckMultisampleQualityLevels(format, maxLevel);
                if (qualityLevels > 0)
                    break;
                maxLevel /= 2;
            }
            return maxLevel;
        }
    }
}
