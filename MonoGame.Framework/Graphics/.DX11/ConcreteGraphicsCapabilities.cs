// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsCapabilities : GraphicsCapabilities
    {

        internal void PlatformInitialize(GraphicsDeviceStrategy deviceStrategy)
        {
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
            SupportsTextureFilterAnisotropic = true;

            SupportsDepth24 = false;
            SupportsPackedDepthStencil = false;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = false;

            // 16bit textures
            SupportsBgra5551 = true;
            SupportsBgra4444 = true;
            SupportsAbgr5551 = false;
            SupportsAbgr4444 = false;

            // Texture compression
            SupportsDxt1 = true;
            SupportsS3tc = true;

            SupportsSRgb = true;

            SupportsTextureArrays = profile >= GraphicsProfile.FL10_0;
            SupportsDepthClamp = profile >= GraphicsProfile.HiDef;

            _maxTextureSlots = 16;
            _maxVertexTextureSlots = (profile >= GraphicsProfile.FL10_0) ? 16 : 0;

            _maxVertexBufferSlots = (profile >= GraphicsProfile.FL10_1) ? 32 : 16;

            SupportsFloatTextures = profile >= GraphicsProfile.HiDef;
            SupportsHalfFloatTextures = profile >= GraphicsProfile.HiDef;
            SupportsNormalized = profile >= GraphicsProfile.HiDef;

            SupportsInstancing = true;
            //TNC: TODO: detect suport based on feture level
            SupportsBaseIndexInstancing = false;
            SupportsSeparateBlendStates = true;

            MaxTextureAnisotropy = (profile == GraphicsProfile.Reach) ? 2 : 16;
        }

    }
}
