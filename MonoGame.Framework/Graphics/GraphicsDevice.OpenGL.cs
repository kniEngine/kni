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

            lock (_strategy.ResourcesLock)
            {
                // Remove references to resources that have been garbage collected.
                for (int i = _strategy.Resources.Count - 1; i >= 0; i--)
                {
                    WeakReference resource = _strategy.Resources[i];

                    if (!resource.IsAlive)
                        _strategy.Resources.RemoveAt(i);
                }
            }
        }

        internal void Android_OnDeviceReset()
        {
            OnDeviceReset(EventArgs.Empty);
        }
    }
}
