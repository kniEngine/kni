// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Javax.Microedition.Khronos.Egl;
using Android.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Log = Android.Util.Log;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private int _glContextCurrentThreadId = -1;
        EGLContext _glSharedContext;

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);


            // create context

            ISurfaceView surfaceView = gameWindow.GameView;
            surfaceView.SurfaceCreated += SurfaceView_SurfaceCreated;
            surfaceView.SurfaceChanged += SurfaceView_SurfaceChanged;
            surfaceView.SurfaceDestroyed += SurfaceView_SurfaceDestroyed;


            if (gameWindow.EglConfig == null)
                gameWindow.GLChooseConfig();

            gameWindow.GLCreateContext();

            if (gameWindow.EglSurface == null)
                gameWindow.GLCreateSurface(adapter, gameWindow.EglConfig);

            if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, gameWindow.EglSurface, gameWindow.EglSurface, gameWindow.EglContext))
                throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());
            _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;
            Threading.MakeMainThread();

            // OGL.InitExtensions() must be called while we have a current gl context.
            if (OGL_DROID.Current.Extensions == null)
                OGL_DROID.Current.InitExtensions();


            // create _glSharedContext for Disposing

            int[] attribs = gameWindow.GLesVersion.GetAttributes();
            _glSharedContext = GL.Egl.EglCreateContext(EGL10.EglNoDisplay, gameWindow.EglConfig, gameWindow.EglContext, attribs);
            if (_glSharedContext != null && _glSharedContext != EGL10.EglNoContext)
                    throw new Exception("Could not create _glSharedContext" + GL.GetEglErrorAsString());


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
                    version = version.Split(' ')[2];
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

            base._capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)base._capabilities).PlatformInitialize(
                 this, ((IPlatformGraphicsContext)this.Context).DeviceStrategy,
                _glMajorVersion, _glMinorVersion);

            base.Initialize(this.Capabilities);

            this.PlatformSetup();
        }

        internal override void EnsureContextCurrentThread()
        {
            Threading.EnsureMainThread();
        }

        public override void BindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, _glSharedContext))
                throw new Exception("Could not Bind DisposeContext" + GL.GetEglErrorAsString());
        }

        public override void UnbindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                throw new Exception("Could not Unbind DisposeContext" + GL.GetEglErrorAsString());
        }


        private void SurfaceView_SurfaceCreated(object sender, EventArgs e)
        {
            ISurfaceView surfaceView = (ISurfaceView)sender;

        }

        private void SurfaceView_SurfaceChanged(object sender, EventArgs e)
        {
            ISurfaceView surfaceView = (ISurfaceView)sender;

            GraphicsDeviceStrategy gds = ((IPlatformGraphicsContext)Context).DeviceStrategy;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gds.PresentationParameters.DeviceWindowHandle);

            if (gameWindow.EglSurface != null)
            {
                ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                // destroy the old _eglSurface
                gameWindow.GlDestroySurface(adapter);
            }
        }

        private void SurfaceView_SurfaceDestroyed(object sender, EventArgs e)
        {
            ISurfaceView surfaceView = (ISurfaceView)sender;

            GraphicsDeviceStrategy gds = ((IPlatformGraphicsContext)Context).DeviceStrategy;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gds.PresentationParameters.DeviceWindowHandle);

            if (gameWindow.EglSurface != null)
            {
                var adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                // destroy the old _eglSurface
                gameWindow.GlDestroySurface(adapter);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            if (_glSharedContext != null)
            {
                //_egl.EglDestroyContext(_eglDisplay, _glSharedContext);
            }
            _glSharedContext = null;


            var gds = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gds.PresentationParameters.DeviceWindowHandle);
            ISurfaceView surfaceView = gameWindow.GameView;
            surfaceView.SurfaceCreated -= SurfaceView_SurfaceCreated;
            surfaceView.SurfaceChanged -= SurfaceView_SurfaceChanged;
            surfaceView.SurfaceDestroyed -= SurfaceView_SurfaceDestroyed;

            if (gameWindow.EglSurface != null)
            {
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                gameWindow.GlDestroySurface(adapter);
            }

            if (gameWindow.EglContext != null)
            {
                if (gameWindow.EglContext != null)
                {
                    if (!GL.Egl.EglDestroyContext(adapter.EglDisplay, gameWindow.EglContext))
                        throw new Exception("Could not destroy EGL context" + GL.GetEglErrorAsString());
                }
                gameWindow._eglContext = null;
            }

            base.Dispose(disposing);
        }

    }
}
