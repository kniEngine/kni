// MonoGame - Copyright (C) The MonoGame Team
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


        internal IntPtr GlContext { get { return _glContext; } }

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContext = SDL.OpenGL.CreateGLContext(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);

            // create _glDisposeContext for Disposing GL objects from GC Finalizer thread.
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextReleaseBehaviour, 0);
            Sdl.Current.OpenGL.SetAttribute(Sdl.GL.Attribute.ShareWithCurrentContext, 1);
            _glDisposeContextWindowHandle = SDL.WINDOW.Create("", 0, 0, 0, 0, Sdl.Window.State.Hidden | Sdl.Window.State.OpenGL);
            _glDisposeContext = SDL.OpenGL.CreateGLContext(_glDisposeContextWindowHandle);

            MakeCurrent(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(((IPlatformGraphicsContext)context).DeviceStrategy.PresentationParameters.PresentationInterval);
            SDL.OpenGL.SetSwapInterval(swapInterval);


            // get the GL version.
            _glMajorVersion = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>().glMajorVersion;
            _glMinorVersion = ((IPlatformGraphicsAdapter)GraphicsAdapter.DefaultAdapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>().glMinorVersion;

            base._capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)base._capabilities).PlatformInitialize(
                this, ((IPlatformGraphicsContext)this.Context).DeviceStrategy,
                _glMajorVersion, _glMinorVersion);

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

            if (_glContext != IntPtr.Zero)
                SDL.OpenGL.DeleteContext(_glContext);
            _glContext = IntPtr.Zero;


            base.Dispose(disposing);
        }

    }
}
