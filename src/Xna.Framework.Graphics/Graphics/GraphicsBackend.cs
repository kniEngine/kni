// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum GraphicsBackend
    {
        OpenGL    = 0x0011,
        GLES      = 0x0012,
        WebGL     = 0x0014,

        DirectX11 = 0x0021,
        DirectX12 = 0x0022,

        WebGPU    = 0x0041,
        Vulkan    = 0x0081,
        Metal     = 0x0101,
    }
}
