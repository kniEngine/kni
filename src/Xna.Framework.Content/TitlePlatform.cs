// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework
{
    public enum TitlePlatform
    {
        Windows    = 0x0011,
        UAP        = 0x0012,
        SDL        = 0x0014, // DesktopGL

        Android    = 0x0021,
        iOS        = 0x0022,
        tvOS       = 0x0024,

        BlazorWASM = 0x0031,

        Oculus     = 0x0041,

        XboxOne        = 0x0051,
        NintendoSwitch = 0x0052,
        PlayStation5   = 0x0054,
    }
}
