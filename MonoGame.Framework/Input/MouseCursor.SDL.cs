// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouseCursor : MouseCursorStrategy
    {
        private static Sdl SDL { get { return Sdl.Current; } }

        public ConcreteMouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            this._cursorType = cursorType;

            Sdl.Mouse.SystemCursor cursor = CursorTypeToSDLCursor(cursorType);
            this._handle = SDL.MOUSE.CreateSystemCursor(cursor);
        }

        private Sdl.Mouse.SystemCursor CursorTypeToSDLCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case MouseCursorStrategy.MouseCursorType.Arrow:
                    return Sdl.Mouse.SystemCursor.Arrow;
                case MouseCursorStrategy.MouseCursorType.IBeam:
                    return Sdl.Mouse.SystemCursor.IBeam;
                case MouseCursorStrategy.MouseCursorType.Wait:
                    return Sdl.Mouse.SystemCursor.Wait;
                case MouseCursorStrategy.MouseCursorType.Crosshair:
                    return Sdl.Mouse.SystemCursor.Crosshair;
                case MouseCursorStrategy.MouseCursorType.WaitArrow:
                    return Sdl.Mouse.SystemCursor.WaitArrow;
                case MouseCursorStrategy.MouseCursorType.SizeNWSE:
                    return Sdl.Mouse.SystemCursor.SizeNWSE;
                case MouseCursorStrategy.MouseCursorType.SizeNESW:
                    return Sdl.Mouse.SystemCursor.SizeNESW;
                case MouseCursorStrategy.MouseCursorType.SizeWE:
                    return Sdl.Mouse.SystemCursor.SizeWE;
                case MouseCursorStrategy.MouseCursorType.SizeNS:
                    return Sdl.Mouse.SystemCursor.SizeNS;
                case MouseCursorStrategy.MouseCursorType.SizeAll:
                    return Sdl.Mouse.SystemCursor.SizeAll;
                case MouseCursorStrategy.MouseCursorType.No:
                    return Sdl.Mouse.SystemCursor.No;
                case MouseCursorStrategy.MouseCursorType.Hand:
                    return Sdl.Mouse.SystemCursor.Hand;

                default:
                    throw new InvalidOperationException("cursorType");
            }
        }


        public ConcreteMouseCursor(byte[] data, int w, int h, int originx, int originy)
        {
            IntPtr surface = IntPtr.Zero;

            try
            {
                surface = SDL.CreateRGBSurfaceFrom(data, w, h, 32, w * 4, 0x000000ff, 0x0000FF00, 0x00FF0000, 0xFF000000);
                if (surface == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create surface for mouse cursor: " + SDL.GetError());

                IntPtr handle = SDL.MOUSE.CreateColorCursor(surface, originx, originy);
                if (handle == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to set surface for mouse cursor: " + SDL.GetError());

                this._cursorType = MouseCursorStrategy.MouseCursorType.User;
                this._handle = handle;
            }
            finally
            {
                if (surface != IntPtr.Zero)
                    SDL.FreeSurface(surface);
            }
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
            }

            if (this.Handle != IntPtr.Zero)
                SDL.MOUSE.FreeCursor(this.Handle);

            base.Dispose(dispose);
        }

    }
}
