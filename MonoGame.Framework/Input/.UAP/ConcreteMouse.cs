// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        internal GameWindow PrimaryWindow;

        public override IntPtr PlatformGetWindowHandle()
        {
            return IntPtr.Zero;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        public override bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        public override MouseState PlatformGetState()
        {
            if (this.PrimaryWindow != null)
            {
                return this.PlatformGetState(this.PrimaryWindow);
            }
            else
                return new MouseState();
        }

        private MouseState PlatformGetState(GameWindow window)
        {
            return window.MouseState;
        }

        public override void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
