// Copyright (C)2023-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Javax.Microedition.Khronos.Egl;
using Log = Android.Util.Log;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : ConcreteGraphicsDeviceGL
    {

        private EGLSurface _eglSurface;

        internal EGLSurface EglSurface { get { return _eglSurface; } }


        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }

        internal void GLCreateSurface(EGLConfig eglConfig)
        {
            ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

            System.Diagnostics.Debug.Assert(this.EglSurface == null);

            OGL_DROID GL = adapter.Ogl;

            _eglSurface = GL.Egl.EglCreateWindowSurface(adapter.EglDisplay, eglConfig, (Java.Lang.Object)gameWindow.GameView.Holder, null);
            if (this.EglSurface == EGL10.EglNoSurface)
                _eglSurface = null;

            if (this.EglSurface == null)
                throw new Exception("Could not create EGL window surface" + GL.GetEglErrorAsString());
        }

        internal void GlDestroySurface()
        {
            ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

            System.Diagnostics.Debug.Assert(this.EglSurface != null);

            OGL_DROID GL = adapter.Ogl;

            if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, this.EglSurface))
                Log.Verbose("AndroidGameWindow", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
            _eglSurface = null;
        }

        public override void Present()
        {
            base.Present();

            IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

            var adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            bool SwapBuffersResult = GL.Egl.EglSwapBuffers(adapter.EglDisplay, this.EglSurface);
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
