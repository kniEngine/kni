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

            var gd = ((IPlatformGraphicsContext)this.Context).DeviceStrategy;
            var adapter = ((IPlatformGraphicsAdapter)gd.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;
            AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);

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


        }

        public override void PlatformSetup()
        {
            base._newEnabledVertexAttributes = new bool[base.Capabilities.MaxVertexBufferSlots];

            if (((ConcreteGraphicsCapabilities)base.Capabilities).SupportsFramebufferObjectARB
            ||  ((ConcreteGraphicsCapabilities)base.Capabilities).SupportsFramebufferObjectEXT)
            {
                base._supportsBlitFramebuffer = GL.BlitFramebuffer != null;
                base._supportsInvalidateFramebuffer = GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "GraphicsDevice requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }
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
