﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private Sdl SDL { get { return Sdl.Current; } }

        private IntPtr _glContext;

        private int _glContextCurrentThreadId = -1;
        private IntPtr _glDisposeContext;
        private IntPtr _glDisposeContextWindowHandle;
        SemaphoreSlim _sharedContextLock = new SemaphoreSlim(1, 1);
        private IntPtr _glSharedContext;
        private IntPtr _glSharedContextWindowHandle;


        internal IntPtr GlContext { get { return _glContext; } }

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContext = SDL.OpenGL.CreateGLContext(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);

            // create _glDisposeContext for Disposing GL objects from GC Finalizer thread.
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextReleaseBehaviour, (int)Sdl.GL.ContextReleaseBehaviour.None);
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ShareWithCurrentContext, 1);
            _glDisposeContextWindowHandle = SDL.WINDOW.Create("", 0, 0, 0, 0, Sdl.Window.State.Hidden | Sdl.Window.State.OpenGL);
            _glDisposeContext = SDL.OpenGL.CreateGLContext(_glDisposeContextWindowHandle);

           // create _glSharedContext for creating GL objects from working threads.
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextReleaseBehaviour, (int)Sdl.GL.ContextReleaseBehaviour.None);
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ShareWithCurrentContext, 1);
            _glSharedContextWindowHandle = SDL.WINDOW.Create("", 0, 0, 0, 0, Sdl.Window.State.Hidden | Sdl.Window.State.OpenGL);
            _glSharedContext = SDL.OpenGL.CreateGLContext(_glSharedContextWindowHandle);

            MakeCurrent(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.PresentationInterval);
            SDL.OpenGL.SetSwapInterval(swapInterval);


            // get the GL version.
            _glVersion = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>().glVersion;

            base._capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)base._capabilities).PlatformInitialize(
                this, ((IPlatformGraphicsContext)this.Context).DeviceStrategy,
                _glVersion);

            base.Initialize(this.Capabilities);

            this.PlatformSetup();
        }

        public void MakeCurrent(IntPtr winHandle)
        {
            SDL.OpenGL.MakeCurrent(winHandle, _glContext);
            _glContextCurrentThreadId = base.ManagedThreadId();
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

            Sdl.Current.OpenGL.MakeCurrent(this._glDisposeContextWindowHandle, this._glDisposeContext);
        }

        public override void UnbindDisposeContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            Sdl.Current.OpenGL.MakeCurrent(IntPtr.Zero, IntPtr.Zero);
        }

        public override bool BindSharedContext()
        {
#if !ENABLE_SHARED_GL_CONTEXT
            EnsureContextCurrentThread();
#endif

            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return false;

            _sharedContextLock.Wait();
            Sdl.Current.OpenGL.MakeCurrent(this._glSharedContextWindowHandle, this._glSharedContext);

            return true;
        }

        public override void UnbindSharedContext()
        {
            if (_glContextCurrentThreadId == base.ManagedThreadId())
                return;

            Sdl.Current.OpenGL.MakeCurrent(IntPtr.Zero, IntPtr.Zero);
            _sharedContextLock.Release();
        }

        /// <summary>
        /// Converts <see cref="PresentInterval"/> to OpenGL swap interval.
        /// </summary>
        /// <returns>A value according to EXT_swap_control</returns>
        /// <param name="interval">The <see cref="PresentInterval"/> to convert.</param>
        internal static int ToGLSwapInterval(PresentInterval interval)
        {
            // See http://www.opengl.org/registry/specs/EXT/swap_control.txt
            // and https://www.opengl.org/registry/specs/EXT/glx_swap_control_tear.txt
            // OpenTK checks for EXT_swap_control_tear:
            // if supported, a swap interval of -1 enables adaptive vsync;
            // otherwise -1 is converted to 1 (vsync enabled.)

            switch (interval)
            {
                case PresentInterval.Immediate:
                    return 0;
                case PresentInterval.One:
                    return 1;
                case PresentInterval.Two:
                    return 2;
                case PresentInterval.Default:

                default:
                    return -1;
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

            }

            if (_glDisposeContext != IntPtr.Zero)
                SDL.OpenGL.DeleteContext(_glDisposeContext);
            _glDisposeContext = IntPtr.Zero;

            if (_glDisposeContextWindowHandle != IntPtr.Zero)
                SDL.WINDOW.Destroy(_glDisposeContextWindowHandle);
            _glDisposeContextWindowHandle = IntPtr.Zero;

            if (_glSharedContext != IntPtr.Zero)
                SDL.OpenGL.DeleteContext(_glSharedContext);
            _glSharedContext = IntPtr.Zero;

            if (_glSharedContextWindowHandle != IntPtr.Zero)
                SDL.WINDOW.Destroy(_glSharedContextWindowHandle);
            _glSharedContextWindowHandle = IntPtr.Zero;

            if (_glContext != IntPtr.Zero)
                SDL.OpenGL.DeleteContext(_glContext);
            _glContext = IntPtr.Zero;


            base.Dispose(disposing);
        }

    }
}
