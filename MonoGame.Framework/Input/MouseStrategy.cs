// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class MouseStrategy
    {
        public abstract IntPtr PlatformGetWindowHandle();
        public abstract void PlatformSetWindowHandle(IntPtr value);
        public abstract bool PlatformIsRawInputAvailable();
        public abstract MouseState PlatformGetState();
        public abstract void PlatformSetPosition(int x, int y);
        public abstract void PlatformSetCursor(MouseCursor cursor);
    }
}