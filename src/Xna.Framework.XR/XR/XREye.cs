// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.XR
{
    public enum XREye
    {
        /// <remarks>
        /// The default when not in XR mode, but also returned by WebXR when in 'immersive-ar' on mobile.
        /// </remarks>
        None  = 0,
        Left  = 1,
        Right = 2,
    }
}
