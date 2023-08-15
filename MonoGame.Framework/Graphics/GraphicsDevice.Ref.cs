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

        internal void OnPresentationChanged()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
