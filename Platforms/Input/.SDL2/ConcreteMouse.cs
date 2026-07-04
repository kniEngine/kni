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

        private int RawX;
        private int RawY;
        internal int ScrollX;
        internal int ScrollY;

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
            Point mousePos;
            Point relPos;
            Sdl.Mouse.Button state;
            IntPtr wndHandle = _wndHandle;
            if (wndHandle != IntPtr.Zero)
            {
                SdlGameWindow gameWindow = SdlGameWindow.FromHandle(wndHandle);

                int winFlags = SDL.WINDOW.GetWindowFlags(wndHandle);

#if ENABLE_TOUCHINPUT
                if (SDL.SDLInitThreadId == SDL.GetManagedThreadId())
                    SDL.PumpEvents();
                state = SDL.MOUSE.GetState(out mousePos.X, out mousePos.Y);
#else
                Point globalPos, windowPos;
                state = SDL.MOUSE.GetGlobalState(out globalPos.X, out globalPos.Y);
                SDL.WINDOW.GetPosition(wndHandle, out windowPos.X, out windowPos.Y);
                mousePos = globalPos - windowPos;
#endif
            }
            else // (wndHandle == IntPtr.Zero)
            {
                state = SDL.MOUSE.GetGlobalState(out mousePos.X, out mousePos.Y);
            }

            SDL.MOUSE.GetRelativeState(out relPos.X, out relPos.Y);
            unchecked
            {
                RawX += relPos.X;
                RawY += relPos.Y;
            }

            MouseState mouseState = new MouseState(
                x: mousePos.X, y: mousePos.Y,
                scrollWheel: ScrollY, horizontalScrollWheel: ScrollX,
                rawX: RawX, rawY: RawY,
                leftButton:   (state & Sdl.Mouse.Button.Left)   != 0 ? ButtonState.Pressed : ButtonState.Released,
                middleButton: (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released,
                rightButton:  (state & Sdl.Mouse.Button.Right)  != 0 ? ButtonState.Pressed : ButtonState.Released,
                xButton1: (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released,
                xButton2: (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released
                );

            return mouseState;
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
