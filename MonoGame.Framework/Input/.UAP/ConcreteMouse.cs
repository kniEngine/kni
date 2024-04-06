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
        private IntPtr _wndHandle = IntPtr.Zero;

        public override IntPtr PlatformGetWindowHandle()
        {
            return _wndHandle;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            _wndHandle = windowHandle;
        }

        public override bool PlatformIsRawInputAvailable()
        {
            return false;
        }

        public override MouseState PlatformGetState()
        {

            IntPtr wndHandle = this._wndHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = UAPGameWindow.FromHandle(wndHandle);
            
                return gameWindow.MouseState;
            }
            else
                return new MouseState();
        }

        public override void PlatformSetPosition(int x, int y)
        {
            GameWindow gameWindow = UAPGameWindow.FromHandle(this._wndHandle);
            gameWindow.MouseState.X = x;
            gameWindow.MouseState.Y = y;
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
