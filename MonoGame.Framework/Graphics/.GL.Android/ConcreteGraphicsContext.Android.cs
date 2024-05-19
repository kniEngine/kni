// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Javax.Microedition.Khronos.Egl;
using Android.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private int _glContextCurrentThreadId = -1;
        EGLContext _glSharedContext;

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;

            var GL = ((OGL_DROID)OGL_DROID.Current);
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            ISurfaceView view = gameWindow.GameView;

            int[] attribs = view.GLesVersion.GetAttributes();
            _glSharedContext = GL.Egl.EglCreateContext(EGL10.EglNoDisplay, view.EglConfig, view.EglContext, attribs);
            if (_glSharedContext != null && _glSharedContext != EGL10.EglNoContext)
                    throw new Exception("Could not create _glSharedContext" + GL.GetEglErrorAsString());
        }

        public override void BindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            var GL = ((OGL_DROID)OGL_DROID.Current);
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            ISurfaceView view = gameWindow.GameView;

            if (!GL.Egl.EglMakeCurrent(view.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, _glSharedContext))
                throw new Exception("Could not Bind DisposeContext" + GL.GetEglErrorAsString());
        }

        public override void UnbindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            var GL = ((OGL_DROID)OGL_DROID.Current);
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            ISurfaceView view = gameWindow.GameView;

            if (!GL.Egl.EglMakeCurrent(view.EglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                throw new Exception("Could not Unbind DisposeContext" + GL.GetEglErrorAsString());
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

            base.Dispose(disposing);
        }

    }
}
