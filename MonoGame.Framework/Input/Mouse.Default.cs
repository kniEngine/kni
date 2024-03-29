// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Mouse
    {
        private IntPtr PlatformGetWindowHandle()
        {
            return IntPtr.Zero;
        }

        private void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        private bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        private MouseState PlatformGetState()
        {
            throw new NotImplementedException();
        }

        private MouseState PlatformGetState(GameWindow window)
        {
            return window.MouseState;
        }

        private void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
        }

        private void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
