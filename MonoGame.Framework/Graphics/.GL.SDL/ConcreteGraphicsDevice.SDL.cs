// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : ConcreteGraphicsDeviceGL
    {

        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        internal void GetModeSwitchedSize(out int width, out int height)
        {
            var mode = new Sdl.Display.Mode
            {
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
                Format = 0,
                RefreshRate = 0,
                DriverData = IntPtr.Zero
            };
            Sdl.Display.Mode closest;
            Sdl.Current.DISPLAY.GetClosestDisplayMode(0, mode, out closest);
            width = closest.Width;
            height = closest.Height;
        }

        internal void GetDisplayResolution(out int width, out int height)
        {
            Sdl.Display.Mode mode;
            Sdl.Current.DISPLAY.GetCurrentDisplayMode(0, out mode);
            width = mode.Width;
            height = mode.Height;
        }


        public override void Present()
        {
            base.Present();

            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            Sdl.Current.OpenGL.SwapWindow(this.PresentationParameters.DeviceWindowHandle);
            GL.CheckGLError();
        }


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
#if DEBUG
            // create debug context, so we get better error messages (glDebugMessageCallback)
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextFlags, 1); // 1 = SDL_GL_CONTEXT_DEBUG_FLAG
#endif

            return new ConcreteGraphicsContext(context);
        }


        internal void PlatformSetup()
        {
            _mainContext = new GraphicsContext(this);
            var GL = _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

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

            _capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)_capabilities).PlatformInitialize(this, _glMajorVersion, _glMinorVersion);


#if DESKTOPGL
            // Initialize draw buffer attachment array
            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers = new DrawBuffersEnum[((ConcreteGraphicsCapabilities)this.Capabilities).MaxDrawBuffers];
            for (int i = 0; i < _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers.Length; i++)
                _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._drawBuffers[i] = (DrawBuffersEnum)(DrawBuffersEnum.ColorAttachment0 + i);
#endif

            _mainContext.Strategy.ToConcrete<ConcreteGraphicsContext>()._newEnabledVertexAttributes = new bool[this.Capabilities.MaxVertexBufferSlots];
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
