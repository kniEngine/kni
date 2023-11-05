// Copyright (C)2023 Nick Kastellanos

namespace Microsoft.Xna.Framework.Graphics
{
    internal interface IRasterizerStateStrategy
    {
        CullMode CullMode { get; set; }
        float DepthBias { get; set; }
        FillMode FillMode { get; set; }
        bool MultiSampleAntiAlias { get; set; }
        bool ScissorTestEnable { get; set; }
        float SlopeScaleDepthBias { get; set; }
        bool DepthClipEnable { get; set; }
    }
}
