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

        internal void OnPresentationChanged()
        {
#if DESKTOPGL
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy).MakeCurrent(_strategy.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(_strategy.PresentationParameters.PresentationInterval);
            Sdl.Current.OpenGL.SetSwapInterval(swapInterval);
#endif

            _strategy._mainContext.ApplyRenderTargets(null);
        }

        internal void Android_OnDeviceResetting()
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(this, EventArgs.Empty);

            lock (_strategy.ResourcesLock)
            {
                foreach (WeakReference resource in _strategy.Resources)
                {
                    GraphicsResource target = resource.Target as GraphicsResource;
                    if (target != null)
                        target.GraphicsDeviceResetting();
                }

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
            var handler = DeviceReset;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

    }
}
