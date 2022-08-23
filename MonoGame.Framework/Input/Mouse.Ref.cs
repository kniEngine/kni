// Copyright (C)2022 Nick Kastellanos


using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {

        private static IntPtr PlatformGetWindowHandle()
        {
            throw new PlatformNotSupportedException();
        }

        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            throw new PlatformNotSupportedException();
        }

        private static bool PlatformIsRawInputAvailable()
        {
            throw new PlatformNotSupportedException();
        }

        private static MouseState PlatformGetState()
        {
            throw new PlatformNotSupportedException();
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            throw new PlatformNotSupportedException();
        }

        private static void PlatformSetPosition(int x, int y)
        {
            throw new PlatformNotSupportedException();
        }

        private static void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
