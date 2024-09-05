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

            var GL = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            Sdl.Current.OpenGL.SwapWindow(this.PresentationParameters.DeviceWindowHandle);
            GL.CheckGLError();
        }


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
#if DEBUG
            // create debug context, so we get better error messages (glDebugMessageCallback)
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextFlags, 1); // 1 = SDL_GL_CONTEXT_DEBUG_FLAG
#endif

            return new ConcreteGraphicsContext(context);
        }


        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            _mainContext = base.CreateGraphicsContext();

            ((IPlatformGraphicsContext)_mainContext).Strategy.PlatformSetup();
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
