// Copyright (C)2022-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        public override IntPtr PlatformGetWindowHandle()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformIsRawInputAvailable()
        {
            throw new PlatformNotSupportedException();
        }

        public override MouseState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetPosition(int x, int y)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
