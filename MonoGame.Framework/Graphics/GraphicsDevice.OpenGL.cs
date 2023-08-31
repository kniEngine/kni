// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal void Android_OnDeviceResetting()
        {
            OnDeviceResetting(EventArgs.Empty);
        }

        internal void Android_OnDeviceReset()
        {
            OnDeviceReset(EventArgs.Empty);
        }
    }
}
