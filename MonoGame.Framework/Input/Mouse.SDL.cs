// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class Mouse
    {
        private Sdl SDL { get { return Sdl.Current; } }

        internal int ScrollX;
        internal int ScrollY;

        private IntPtr PlatformGetWindowHandle()
        {
            return PrimaryWindow.Handle;
        }
        
        private void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        private bool PlatformIsRawInputAvailable()
        {
            return true;
        }

        private MouseState PlatformGetState()
        {
            throw new NotImplementedException();
        }

        private MouseState PlatformGetState(GameWindow window)
        {
            int x, y;
            var winFlags = SDL.WINDOW.GetWindowFlags(window.Handle);
            var state = SDL.MOUSE.GetGlobalState(out x, out y);
            var clientBounds = window.ClientBounds;

            window.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

            window.MouseState.HorizontalScrollWheelValue = ScrollX;
            window.MouseState.ScrollWheelValue = ScrollY;

            window.MouseState.X = x - clientBounds.X;
            window.MouseState.Y = y - clientBounds.Y;

            return window.MouseState;
        }

        private void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;

            SDL.MOUSE.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        private void PlatformSetCursor(MouseCursor cursor)
        {
            SDL.MOUSE.SetCursor(cursor.Handle);
        }
    }
}
