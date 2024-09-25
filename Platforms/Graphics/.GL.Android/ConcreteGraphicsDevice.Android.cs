// Copyright (C)2023 Nick Kastellanos

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

            ISurfaceView surfaceView = gameWindow.GameView;

            bool SwapBuffersResult = GL.Egl.EglSwapBuffers(adapter.EglDisplay, surfaceView.EglSurface);
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

        internal void Android_OnContextLost()
        {
            this.OnContextLost(EventArgs.Empty);
        }

        internal void Android_OnDeviceReset()
        {
            this.OnDeviceReset(EventArgs.Empty);
        }

        internal void Android_UpdateBackBufferBounds(int viewWidth, int viewHeight)
        {
            this.PresentationParameters.BackBufferWidth = viewWidth;
            this.PresentationParameters.BackBufferHeight = viewHeight;

            // Set the viewport from PresentationParameters
            if (!((IPlatformGraphicsContext)this.MainContext).Strategy.IsRenderTargetBound)
            {
                PresentationParameters pp2 = this.PresentationParameters;
                this.MainContext.Viewport = new Viewport(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
                this.MainContext.ScissorRectangle = new Rectangle(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
            }
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
