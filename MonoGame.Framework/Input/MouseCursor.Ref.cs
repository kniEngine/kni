// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {

        private MouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            throw new PlatformNotSupportedException();
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
