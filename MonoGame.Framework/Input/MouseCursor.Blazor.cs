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


        public MouseCursor(byte[] data, int w, int h, int originx, int originy)
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
