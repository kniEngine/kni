// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private readonly MouseCursorType _cursorType;
        
        private IntPtr PlatformGetHandle()
        {
            throw new PlatformNotSupportedException();
        }

        private bool PlatformIsBuildInMouseCursor
        {
            get { throw new PlatformNotSupportedException(); }
        }


        private MouseCursor(MouseCursorType cursorType)
        {
            throw new PlatformNotSupportedException();
        }


        private static void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }

        private static MouseCursor PlatformFromTexture2D(byte[] data, int w, int h, int originx, int originy)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDispose(bool dispose)
        {
            if (dispose)
            {
            }

        }
    }
}
