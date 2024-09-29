// Copyright (C)2023-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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


        public override void Present()
        {
            base.Present();

            IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

            var adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            bool SwapBuffersResult = GL.Egl.EglSwapBuffers(adapter.EglDisplay, gameWindow.EglSurface);
            if (!SwapBuffersResult)
            {
                int eglError = GL.Egl.EglGetError();
                System.Diagnostics.Debug.WriteLine("SwapBuffers failed." + GL.GetEglErrorAsString());
            }
        }


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
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
