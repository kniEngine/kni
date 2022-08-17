// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {

        private void PlatformGraphicsDeviceResetting()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }            

            base.Dispose(disposing);
        }

    }

}

