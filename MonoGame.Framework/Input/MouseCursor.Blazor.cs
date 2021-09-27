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


        private MouseCursor(bool isBuildInMouseCursor)
        {
            _isBuildInMouseCursor = isBuildInMouseCursor;
            _handle = IntPtr.Zero;
        }


        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(true);
            IBeam = new MouseCursor(true);
            Wait = new MouseCursor(true);
            Crosshair = new MouseCursor(true);
            WaitArrow = new MouseCursor(true);
            SizeNWSE = new MouseCursor(true);
            SizeNESW = new MouseCursor(true);
            SizeWE = new MouseCursor(true);
            SizeNS = new MouseCursor(true);
            SizeAll = new MouseCursor(true);
            No = new MouseCursor(true);
            Hand = new MouseCursor(true);
        }

        private static MouseCursor PlatformFromTexture2D(byte[] data, int w, int h, int originx, int originy)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformDispose(bool dispose)
        {
        }
    }
}
