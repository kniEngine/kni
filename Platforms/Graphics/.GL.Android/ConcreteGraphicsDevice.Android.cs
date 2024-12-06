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

        private EGLConfig _eglConfig;
        private EGLSurface _eglSurface;

        internal EGLConfig EglConfig { get { return _eglConfig; } }
        internal EGLSurface EglSurface { get { return _eglSurface; } }


        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        internal void GLChooseConfig()
        {
#if CARDBOARD
            // Cardboard: EglConfig was created by GLSurfaceView.            
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(this.PresentationParameters.DeviceWindowHandle);
            _eglConfig = gameWindow.EglConfig;
            return;
#endif

            var adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            int depth = 0;
            int stencil = 0;
            int sampleBuffers = 0;
            int samples = 0;
            switch (this.PresentationParameters.DepthStencilFormat)
            {
                case DepthFormat.Depth16:
                    depth = 16;
                    break;
                case DepthFormat.Depth24:
                    depth = 24;
                    break;
                case DepthFormat.Depth24Stencil8:
                    depth = 24;
                    stencil = 8;
                    break;
                case DepthFormat.None:
                    break;
            }

            if (this.PresentationParameters.MultiSampleCount > 1)
            {
                sampleBuffers = 1;
                samples = 4;
            }

            List<SurfaceConfig> surfaceConfigs = new List<SurfaceConfig>();
            if (depth > 0)
            {
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil, SampleBuffers = sampleBuffers, Samples = samples });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                surfaceConfigs.Add(new SurfaceConfig() { Depth = depth, Stencil = stencil });
                if (depth > 16)
                {
                    surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    surfaceConfigs.Add(new SurfaceConfig() { Depth = 16 });
                }
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            else
            {
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, SampleBuffers = sampleBuffers, Samples = samples });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                surfaceConfigs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            surfaceConfigs.Add(new SurfaceConfig() { Red = 4, Green = 4, Blue = 4 });
            EGLConfig[] results = new EGLConfig[1];

            bool found = false;
            int[] numConfigs = new int[] { 0 };
            foreach (SurfaceConfig surfaceConfig in surfaceConfigs)
            {
                Log.Verbose("AndroidGameView", string.Format("Checking Config : {0}", surfaceConfig));
                found = GL.Egl.EglChooseConfig(adapter.EglDisplay, surfaceConfig.ToConfigAttribs(), results, 1, numConfigs);
                Log.Verbose("AndroidGameView", "EglChooseConfig returned {0} and {1}", found, numConfigs[0]);
                if (!found || numConfigs[0] <= 0)
                {
                    Log.Verbose("AndroidGameView", "Config not supported");
                    continue;
                }
                Log.Verbose("AndroidGameView", string.Format("Selected Config : {0}", surfaceConfig));
                break;
            }

            if (!found || numConfigs[0] <= 0)
                throw new Exception("No valid EGL configs found" + GL.GetEglErrorAsString());
            _eglConfig = results[0];
        }

        internal void GLCreateSurface()
        {
            ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

            System.Diagnostics.Debug.Assert(this.EglSurface == null);

            OGL_DROID GL = adapter.Ogl;

#if CARDBOARD
            // Cardboard: EglSurface and EglContext was created by GLSurfaceView.
            _eglSurface = GL.Egl.EglGetCurrentSurface(EGL10.EglDraw);
            if (this.EglSurface == EGL10.EglNoSurface)
                _eglSurface = null;
            return;
#endif

            _eglSurface = GL.Egl.EglCreateWindowSurface(adapter.EglDisplay, this.EglConfig, (Java.Lang.Object)gameWindow.GameView.Holder, null);
            if (this.EglSurface == EGL10.EglNoSurface)
                _eglSurface = null;

            if (this.EglSurface == null)
                throw new Exception("Could not create EGL window surface" + GL.GetEglErrorAsString());
        }

        internal void GlDestroySurface()
        {
            ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)this.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();

            System.Diagnostics.Debug.Assert(this.EglSurface != null);

            OGL_DROID GL = adapter.Ogl;

            if (!GL.Egl.EglDestroySurface(adapter.EglDisplay, this.EglSurface))
                Log.Verbose("AndroidGameWindow", "Could not destroy EGL surface" + GL.GetEglErrorAsString());
            _eglSurface = null;
        }

        public override void Present()
        {
            base.Present();

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
