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

        private readonly bool _isBuildInMouseCursor;
        private IntPtr _handle;
        
        private IntPtr PlatformGetHandle()
        {
            return _handle;
        }

        private bool PlatformIsBuildInMouseCursor
        {
            get { return _isBuildInMouseCursor; }
        }


        private MouseCursor(bool isBuildInMouseCursor, IntPtr handle)
        {
            _isBuildInMouseCursor = isBuildInMouseCursor;
            _handle = handle;
        }

        private MouseCursor(bool isBuildInMouseCursor, Sdl.Mouse.SystemCursor cursor)
        {
            _isBuildInMouseCursor = isBuildInMouseCursor;
            _handle = SDL.MOUSE.CreateSystemCursor(cursor);
        }


        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(true, Sdl.Mouse.SystemCursor.Arrow);
            IBeam = new MouseCursor(true, Sdl.Mouse.SystemCursor.IBeam);
            Wait = new MouseCursor(true, Sdl.Mouse.SystemCursor.Wait);
            Crosshair = new MouseCursor(true, Sdl.Mouse.SystemCursor.Crosshair);
            WaitArrow = new MouseCursor(true, Sdl.Mouse.SystemCursor.WaitArrow);
            SizeNWSE = new MouseCursor(true, Sdl.Mouse.SystemCursor.SizeNWSE);
            SizeNESW = new MouseCursor(true, Sdl.Mouse.SystemCursor.SizeNESW);
            SizeWE = new MouseCursor(true, Sdl.Mouse.SystemCursor.SizeWE);
            SizeNS = new MouseCursor(true, Sdl.Mouse.SystemCursor.SizeNS);
            SizeAll = new MouseCursor(true, Sdl.Mouse.SystemCursor.SizeAll);
            No = new MouseCursor(true, Sdl.Mouse.SystemCursor.No);
            Hand = new MouseCursor(true, Sdl.Mouse.SystemCursor.Hand);
        }

        private static MouseCursor PlatformFromTexture2D(byte[] data, int w, int h, int originx, int originy)
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

                return new MouseCursor(false, handle);
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
                SDL.MOUSE.FreeCursor(_handle);

            _handle = IntPtr.Zero;
        }
    }
}
