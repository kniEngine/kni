// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IRenderTargetStrategy
    {

        DepthFormat DepthStencilFormat { get; }
        int MultiSampleCount { get; }
        RenderTargetUsage RenderTargetUsage { get; }

        bool IsContentLost { get; }
    }
}
