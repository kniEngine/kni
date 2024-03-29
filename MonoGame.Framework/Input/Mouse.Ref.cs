// Copyright (C)2022 Nick Kastellanos


using System;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Mouse
    {

        private IntPtr PlatformGetWindowHandle()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            throw new PlatformNotSupportedException();
        }

        private bool PlatformIsRawInputAvailable()
        {
            throw new PlatformNotSupportedException();
        }

        private MouseState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetPosition(int x, int y)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
