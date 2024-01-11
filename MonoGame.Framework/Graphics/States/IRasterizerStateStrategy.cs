// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformRasterizerState
    {
        T GetStrategy<T>() where T : IRasterizerStateStrategy;
    }

    public interface IRasterizerStateStrategy
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
