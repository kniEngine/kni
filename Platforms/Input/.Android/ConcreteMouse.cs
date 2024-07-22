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

        private Point _pos;
        private int _scrollX, _scrollY;
        private int _rawX, _rawY;
        private ButtonState _leftButton, _rightButton, _middleButton;


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
            MouseState mouseState = new MouseState(
                    x: _pos.X, y: _pos.Y,
                    scrollWheel: _scrollY, horizontalScrollWheel: _scrollX,
                    rawX: _rawX, rawY: _rawY,
                    leftButton: _leftButton,
                    middleButton: _middleButton,
                    rightButton: _rightButton,
                    xButton1: ButtonState.Released,
                    xButton2: ButtonState.Released
                    );

            return mouseState;
        }

        public override void PlatformSetPosition(int x, int y)
        {
            _pos.X = x;
            _pos.Y = y;
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
