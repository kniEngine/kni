// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private int _glContextCurrentThreadId = -1;
        OpenGLES.EAGLContext _glDisposeContext;
        SemaphoreSlim _sharedContextLock = new SemaphoreSlim(1, 1);
        OpenGLES.EAGLContext _glSharedContext;

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContextCurrentThreadId = base.ManagedThreadId();

            iOSGameWindow gameWindow = iOSGameWindow.FromHandle(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            iOSGameViewController viewController = gameWindow.ViewController;
            iOSGameView view = viewController.View;


            // create _glDisposeContext for Disposing GL objects from GC Finalizer thread.
            try
            {
                _glDisposeContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES3, viewController.View._eaglContext.ShareGroup);
            }
            catch
            {
                // Fall back to GLES 2.0
                _glDisposeContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES2, viewController.View._eaglContext.ShareGroup);
            }

            // create _glSharedContext for creating GL objects from working threads.
            try
            {
                _glSharedContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES3, viewController.View._eaglContext.ShareGroup);
            }
            catch
            {
                // Fall back to GLES 2.0
                _glSharedContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES2, viewController.View._eaglContext.ShareGroup);
            }


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
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            throw new InvalidOperationException("Operation not called on main thread.");
        }

        public override void BindDisposeContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            OpenGLES.EAGLContext.SetCurrentContext(_glDisposeContext);
        }

        public override void UnbindDisposeContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            OpenGLES.EAGLContext.SetCurrentContext(null);
        }

        public override bool BindSharedContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return false;

            _sharedContextLock.Wait();
            OpenGLES.EAGLContext.SetCurrentContext(_glSharedContext);

            return true;
        }

        public override void UnbindSharedContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            OpenGLES.EAGLContext.SetCurrentContext(null);
            _sharedContextLock.Release();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                if (_glDisposeContext != null)
                {
                    _glDisposeContext.Dispose();
                    _glDisposeContext = null;
                }

                if (_glSharedContext != null)
                {
                    _glSharedContext.Dispose();
                    _glSharedContext = null;
                }
            }

            base.Dispose(disposing);
        }

    }
}
