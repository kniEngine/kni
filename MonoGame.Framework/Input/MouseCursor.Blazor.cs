// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {

        private MouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            _strategy = new MouseCursorStrategy();

            _strategy._cursorType = cursorType;
            _strategy._handle = IntPtr.Zero;

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
