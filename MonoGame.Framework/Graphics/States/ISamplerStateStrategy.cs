// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface ISamplerStateStrategy
    {
        TextureFilter Filter { get; set; }
        TextureAddressMode AddressU { get; set; }
        TextureAddressMode AddressV { get; set; }
        TextureAddressMode AddressW { get; set; }
        Color BorderColor { get; set; }
        int MaxAnisotropy { get; set; }
        int MaxMipLevel { get; set; }
        float MipMapLevelOfDetailBias { get; set; }
        CompareFunction ComparisonFunction { get; set; }
        TextureFilterMode FilterMode { get; set; }
    }
}
