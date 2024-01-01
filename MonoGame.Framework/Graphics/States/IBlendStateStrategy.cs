// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IBlendStateStrategy
    {
        bool IndependentBlendEnable { get; set; }
        TargetBlendState[] Targets { get; }

        Color BlendFactor { get; set; }
        int MultiSampleMask { get; set; }
        BlendFunction AlphaBlendFunction { get; set; }
        Blend AlphaDestinationBlend { get; set; }
        Blend AlphaSourceBlend { get; set; }
        BlendFunction ColorBlendFunction { get; set; }
        Blend ColorDestinationBlend { get; set; }
        Blend ColorSourceBlend { get; set; }
        ColorWriteChannels ColorWriteChannels { get; set; }
        ColorWriteChannels ColorWriteChannels1 { get; set; }
        ColorWriteChannels ColorWriteChannels2 { get; set; }
        ColorWriteChannels ColorWriteChannels3 { get; set; }
    }
}
