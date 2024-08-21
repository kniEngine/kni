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

            try
            {
                IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
                AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

                var adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                ISurfaceView surfaceView = gameWindow.GameView;

                if (!GL.Egl.EglSwapBuffers(adapter.EglDisplay, surfaceView.EglSurface))
                {
                    if (GL.Egl.EglGetError() == 0)
                    {
                        if (gameWindow._isGLContextLost)
                            System.Diagnostics.Debug.WriteLine("Lost EGL context" + GL.GetEglErrorAsString());
                        gameWindow._isGLContextLost = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
        }


        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            _mainContext = base.CreateGraphicsContext();

            var GL = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on GL_VERSION string
            try
            {
                string version = GL.GetString(StringName.Version);
                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                // for GLES, the GL_VERSION string is formatted as:
                //     OpenGL<space>ES<space><version number><space><vendor-specific information>
                if (version.StartsWith("OpenGL ES "))
                    version =  version.Split(' ')[2];
                else // if it fails, we assume to be on a 1.1 context
                    version = "1.1";

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

            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>()._newEnabledVertexAttributes = new bool[this.Capabilities.MaxVertexBufferSlots];
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
