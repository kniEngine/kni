// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {

        private void PlatformSetup()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }        

        private void PlatformDispose()
        {
        }

        internal void PlatformPresent()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new PlatformNotSupportedException();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            throw new PlatformNotSupportedException();
        }        

        internal void OnPresentationChanged()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
