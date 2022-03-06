// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser
{
    public class SamplerStateInfo
    {
        public SamplerStateInfo()
        {
            // NOTE: These match the defaults of SamplerState.
            MinFilter = TextureFilterTypeContent.Linear;
            MagFilter = TextureFilterTypeContent.Linear;
            MipFilter = TextureFilterTypeContent.Linear;
            AddressU = TextureAddressModeContent.Wrap;
            AddressV = TextureAddressModeContent.Wrap;
            AddressW = TextureAddressModeContent.Wrap;
            BorderColor = Color.White;
            MaxAnisotropy = 4;
            MaxMipLevel = 0;
            MipMapLevelOfDetailBias = 0.0f;
        }

        public string Name { get; set; }

        public string TextureName { get; set; }

        public TextureFilterTypeContent MinFilter { get; set; }

        public TextureFilterTypeContent MagFilter { get; set; }

        public TextureFilterTypeContent MipFilter { get; set; }

        public TextureFilterTypeContent Filter
        {
            set { MinFilter = MagFilter = MipFilter = value; }
        }

        public TextureAddressModeContent AddressU { get; set; }
        public TextureAddressModeContent AddressV { get; set; }
        public TextureAddressModeContent AddressW { get; set; }

        public Color BorderColor { get; set; }

        public int MaxAnisotropy { get; set; }

        public int MaxMipLevel { get; set; }

        public float MipMapLevelOfDetailBias { get; set; }
    }
}
