﻿// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.XR
{
    public enum XRDeviceState
    {
        InitializingDevice,
        Disabled,
        InitializingSession,
        Enabled,
        NoPermissions,
    }
}
