// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal int _glMajorVersion = 0;
        internal int _glMinorVersion = 0;


        private void PlatformSetup()
        {
            _strategy._mainContext = new GraphicsContext(_strategy);

            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on GL_VERSION string
            try
            {
                string version = GL.GetString(StringName.Version);
                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                // for OpenGL, the GL_VERSION string always starts with the version number in the "major.minor" format,
                // optionally followed by multiple vendor specific characters
                // for GLES, the GL_VERSION string is formatted as:
                //     OpenGL<space>ES<space><version number><space><vendor-specific information>
#if GLES
                if (version.StartsWith("OpenGL ES "))
                    version =  version.Split(' ')[2];
                else // if it fails, we assume to be on a 1.1 context
                    version = "1.1";
#endif
                _glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
                _glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
            }
            catch (FormatException)
            {
                // if it fails, we assume to be on a 1.1 context
                _glMajorVersion = 1;
                _glMinorVersion = 1;
            }

            Strategy._capabilities = new GraphicsCapabilities();
            Strategy._capabilities.PlatformInitialize(this, _glMajorVersion, _glMinorVersion);


#if DESKTOPGL
            // Initialize draw buffer attachment array
            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers = new DrawBuffersEnum[Strategy.Capabilities.MaxDrawBuffers];
			for (int i = 0; i < ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers.Length; i++)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._drawBuffers[i] = (DrawBuffersEnum)(DrawBuffersEnum.ColorAttachment0 + i);
#endif

            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._newEnabledVertexAttributes = new bool[Strategy.Capabilities.MaxVertexBufferSlots];
        }


        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            ((ConcreteGraphicsDevice)_strategy).ClearProgramCache();

#if DESKTOPGL
            _strategy._mainContext.Dispose();
            _strategy._mainContext = null;
#endif
        }

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
