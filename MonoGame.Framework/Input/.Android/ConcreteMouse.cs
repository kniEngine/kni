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
        private MouseState _mouseState;

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
            return _mouseState;
        }

        public override void PlatformSetPosition(int x, int y)
        {
            _mouseState.X = x;
            _mouseState.Y = y;
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
