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

        private Sdl SDL { get { return Sdl.Current; } }

        internal int ScrollX;
        internal int ScrollY;
        internal int RawX;
        internal int RawY;

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
            return true;
        }

        public override MouseState PlatformGetState()
        {
            IntPtr wndHandle = _wndHandle;
            if (wndHandle != IntPtr.Zero)
            {
                SdlGameWindow gameWindow = SdlGameWindow.FromHandle(wndHandle);

                int winFlags = SDL.WINDOW.GetWindowFlags(wndHandle);

                Point pos, windowPos;
                Sdl.Mouse.Button state = SDL.MOUSE.GetGlobalState(out pos.X, out pos.Y);
                SDL.WINDOW.GetPosition(wndHandle, out windowPos.X, out windowPos.Y);
                Point clientPos = pos - windowPos;

                MouseState mouseState = new MouseState(
                    x: clientPos.X, y: clientPos.Y,
                    scrollWheel: ScrollY, horizontalScrollWheel: ScrollX,
                    rawX: RawX, rawY: RawY,
                    leftButton: (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released,
                    middleButton: (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released,
                    rightButton: (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released,
                    xButton1: (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released,
                    xButton2: (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released
                    );

                return mouseState;
            }
            else
                return new MouseState();
        }

        public override void PlatformSetPosition(int x, int y)
        {
            SdlGameWindow gameWindow = SdlGameWindow.FromHandle(_wndHandle);

            SDL.MOUSE.WarpInWindow(gameWindow.Handle, x, y);
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            SDL.MOUSE.SetCursor(cursor.Handle);
        }

    }
}
