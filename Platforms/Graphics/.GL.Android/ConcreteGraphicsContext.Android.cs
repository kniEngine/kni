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

        private GLESVersion _glesVersion;
        private EGLContext _eglContext;
        private EGLContext _glSharedContext;

        internal GLESVersion GLesVersion { get { return _glesVersion; } }
        internal EGLContext EglContext { get { return _eglContext; } }

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var cgd = gd.ToConcrete<ConcreteGraphicsDevice>();
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gd.PresentationParameters.DeviceWindowHandle);


            // create context

            ISurfaceView surfaceView = gameWindow.GameView;
            surfaceView.SurfaceCreated += SurfaceView_SurfaceCreated;
            surfaceView.SurfaceChanged += SurfaceView_SurfaceChanged;
            surfaceView.SurfaceDestroyed += SurfaceView_SurfaceDestroyed;

            cgd.GLChooseConfig();

            this.GLCreateContext();

            cgd.GLCreateSurface();

            if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, cgd.EglSurface, cgd.EglSurface, this.EglContext))
                throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());
            _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;

            // OGL.InitExtensions() must be called while we have a current gl context.
            if (OGL_DROID.Current.Extensions == null)
                OGL_DROID.Current.InitExtensions();


            // create _glSharedContext for Disposing

            int[] attribs = this.GLesVersion.GetAttributes();
            _glSharedContext = GL.Egl.EglCreateContext(EGL10.EglNoDisplay, cgd.EglConfig, this.EglContext, attribs);
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


        private void GLCreateContext()
        {
            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var cgd = gd.ToConcrete<ConcreteGraphicsDevice>();
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

#if CARDBOARD
            // Cardboard: EglSurface and EglContext was created by GLSurfaceView.
            _eglContext = GL.Egl.EglGetCurrentContext();
            if (this.EglContext == EGL10.EglNoContext)
                _eglContext = null;
#else
            foreach (GLESVersion ver in ((OGL_DROID)OGL.Current).GetSupportedGLESVersions())
            {
                Log.Verbose("ConcreteGraphicsContext", "Creating GLES {0} Context", ver);

                _eglContext = GL.Egl.EglCreateContext(adapter.EglDisplay, cgd.EglConfig, EGL10.EglNoContext, ver.GetAttributes());

                if (this.EglContext == null || this.EglContext == EGL10.EglNoContext)
                {
                    this._eglContext = null;
                    Log.Verbose("ConcreteGraphicsContext", string.Format("GLES {0} Not Supported. {1}", ver, GL.GetEglErrorAsString()));
                    continue;
                }
                _glesVersion = ver;
                break;
            }

            if (this.EglContext == EGL10.EglNoContext) this._eglContext = null;
            if (this.EglContext == null)
                throw new Exception("Could not create EGL context" + GL.GetEglErrorAsString());

            Log.Verbose("ConcreteGraphicsContext", "Created GLES {0} Context", this.GLesVersion);
#endif
        }

        internal EGLSurface GLCreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            EGLSurface result = GL.Egl.EglCreatePbufferSurface(adapter.EglDisplay, config, attribList);

            if (result == EGL10.EglNoSurface)
                result = null;

            if (result == null)
                throw new Exception("EglCreatePBufferSurface");

            return result;
        }


        internal override void EnsureContextCurrentThread()
        {
            if (_glContextCurrentThreadId == Thread.CurrentThread.ManagedThreadId)
                return;

            throw new InvalidOperationException("Operation not called on main thread.");
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
            var cgd = gds.ToConcrete<ConcreteGraphicsDevice>();
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gds.PresentationParameters.DeviceWindowHandle);

            if (cgd.EglSurface != null)
            {
                ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                // destroy the old _eglSurface
                cgd.GlDestroySurface();
            }

            // recreate EglSurface and bind the context to the thread
            {
                ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

#if CARDBOARD
                // Cardboard: EglSurface and EglContext was created by GLSurfaceView.
                _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;
#else
                cgd.GLCreateSurface();

                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, cgd.EglSurface, cgd.EglSurface, this.EglContext))
                {
                    throw new Exception("Could not make EGL current" + GL.GetEglErrorAsString());
                }
                _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;
#endif

                // Update BackBuffer bounds
                int w = gameWindow.GameView.Width;
                int h = gameWindow.GameView.Height;
                gds.PresentationParameters.BackBufferWidth  = w;
                gds.PresentationParameters.BackBufferHeight = h;

                if (!((IPlatformGraphicsContext)gds.MainContext).Strategy.IsRenderTargetBound)
                {
                    gds.MainContext.Viewport = new Viewport(0, 0, w, h);
                    gds.MainContext.ScissorRectangle = new Rectangle(0, 0, w, h);
                }
            }
        }

        private void SurfaceView_SurfaceDestroyed(object sender, EventArgs e)
        {
            GraphicsDeviceStrategy gds = ((IPlatformGraphicsContext)Context).DeviceStrategy;
            var cgd = gds.ToConcrete<ConcreteGraphicsDevice>();

            if (cgd.EglSurface != null)
            {
                var adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                var GL = adapter.Ogl;

                // unbind Context and Surface
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                // destroy the old _eglSurface
                cgd.GlDestroySurface();
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
            var cgd = gds.ToConcrete<ConcreteGraphicsDevice>();
            var adapter = ((IPlatformGraphicsAdapter)gds.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(gds.PresentationParameters.DeviceWindowHandle);
            ISurfaceView surfaceView = gameWindow.GameView;
            surfaceView.SurfaceCreated -= SurfaceView_SurfaceCreated;
            surfaceView.SurfaceChanged -= SurfaceView_SurfaceChanged;
            surfaceView.SurfaceDestroyed -= SurfaceView_SurfaceDestroyed;

            if (cgd.EglSurface != null)
            {
                if (!GL.Egl.EglMakeCurrent(adapter.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    Log.Verbose("ConcreteGraphicsContext", "Could not unbind EGL surface" + GL.GetEglErrorAsString());

                cgd.GlDestroySurface();
            }

            if (this.EglContext != null)
            {
                if (this.EglContext != null)
                {
                    if (!GL.Egl.EglDestroyContext(adapter.EglDisplay, this.EglContext))
                        throw new Exception("Could not destroy EGL context" + GL.GetEglErrorAsString());
                }
                this._eglContext = null;
            }

            base.Dispose(disposing);
        }

    }
}
