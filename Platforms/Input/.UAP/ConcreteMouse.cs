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
                UAPGameWindow gameWindow = UAPGameWindow.FromHandle(wndHandle);
            
                return gameWindow._mouseState;
            }
            else
                return new MouseState();
        }

        public override void PlatformSetPosition(int x, int y)
        {
            UAPGameWindow gameWindow = UAPGameWindow.FromHandle(this._wndHandle);

            MouseState mouseState = gameWindow._mouseState;

            gameWindow._mouseState = new MouseState(
                    x: x,
                    y: y,
                    scrollWheel: mouseState.ScrollWheelValue,
                    horizontalScrollWheel: mouseState.HorizontalScrollWheelValue,
                    rawX: mouseState.RawX,
                    rawY: mouseState.RawY,
                    leftButton: mouseState.LeftButton,
                    middleButton: mouseState.MiddleButton,
                    rightButton: mouseState.RightButton,
                    xButton1: mouseState.XButton1,
                    xButton2: mouseState.XButton2 
                );
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
