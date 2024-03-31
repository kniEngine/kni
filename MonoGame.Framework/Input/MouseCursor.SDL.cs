// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private static Sdl SDL { get { return Sdl.Current; } }

        private readonly MouseCursorType _cursorType;
        private IntPtr _handle;
        
        private IntPtr PlatformGetHandle()
        {
            return _handle;
        }

        private bool PlatformIsBuildInMouseCursor
        {
            get { return _cursorType != MouseCursorType.User; }
        }

        private MouseCursor(MouseCursorType cursorType)
        {
            _cursorType = cursorType;
            _handle = IntPtr.Zero;

            Sdl.Mouse.SystemCursor cursor = CursorTypeToSDLCursor(cursorType);
            _handle = SDL.MOUSE.CreateSystemCursor(cursor);
        }

        private Sdl.Mouse.SystemCursor CursorTypeToSDLCursor(MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case MouseCursorType.Arrow:
                    return Sdl.Mouse.SystemCursor.Arrow;
                case MouseCursorType.IBeam:
                    return Sdl.Mouse.SystemCursor.IBeam;
                case MouseCursorType.Wait:
                    return Sdl.Mouse.SystemCursor.Wait;
                case MouseCursorType.Crosshair:
                    return Sdl.Mouse.SystemCursor.Crosshair;
                case MouseCursorType.WaitArrow:
                    return Sdl.Mouse.SystemCursor.WaitArrow;
                case MouseCursorType.SizeNWSE:
                    return Sdl.Mouse.SystemCursor.SizeNWSE;
                case MouseCursorType.SizeNESW:
                    return Sdl.Mouse.SystemCursor.SizeNESW;
                case MouseCursorType.SizeWE:
                    return Sdl.Mouse.SystemCursor.SizeWE;
                case MouseCursorType.SizeNS:
                    return Sdl.Mouse.SystemCursor.SizeNS;
                case MouseCursorType.SizeAll:
                    return Sdl.Mouse.SystemCursor.SizeAll;
                case MouseCursorType.No:
                    return Sdl.Mouse.SystemCursor.No;
                case MouseCursorType.Hand:
                    return Sdl.Mouse.SystemCursor.Hand;

                default:
                    throw new InvalidOperationException("cursorType");
            }
        }


        public MouseCursor(byte[] data, int w, int h, int originx, int originy)
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

                _cursorType = MouseCursorType.User;
                _handle = handle;
            }
            finally
            {
                if (surface != IntPtr.Zero)
                    SDL.FreeSurface(surface);
            }
        }

        private void PlatformDispose(bool dispose)
        {
            if (dispose)
            {
            }

            if (_handle != IntPtr.Zero)
            {
                SDL.MOUSE.FreeCursor(_handle);
                _handle = IntPtr.Zero;
            }

        }

    }
}
