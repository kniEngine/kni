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
        }


        private static void PlatformInitalize()
        {
            Arrow     = new MouseCursor(MouseCursorType.Arrow);
            IBeam     = new MouseCursor(MouseCursorType.IBeam);
            Wait      = new MouseCursor(MouseCursorType.Wait);
            Crosshair = new MouseCursor(MouseCursorType.Crosshair);
            WaitArrow = new MouseCursor(MouseCursorType.WaitArrow);
            SizeNWSE  = new MouseCursor(MouseCursorType.SizeNWSE);
            SizeNESW  = new MouseCursor(MouseCursorType.SizeNESW);
            SizeWE    = new MouseCursor(MouseCursorType.SizeWE);
            SizeNS    = new MouseCursor(MouseCursorType.SizeNS);
            SizeAll   = new MouseCursor(MouseCursorType.SizeAll);
            No        = new MouseCursor(MouseCursorType.No);
            Hand      = new MouseCursor(MouseCursorType.Hand);
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
