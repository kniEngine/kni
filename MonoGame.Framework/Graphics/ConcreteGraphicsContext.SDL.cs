// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private IntPtr _glContext;

        internal IntPtr GlContext { get { return _glContext; } }

        internal ConcreteGraphicsContext(GraphicsDevice device, IntPtr sdlWindowHandle)
            : base(device)
        {
            _glContext = Sdl.GL.CreateGLContext(sdlWindowHandle);

            // GL entry points must be loaded after the GL context creation, otherwise some Windows drivers will return only GL 1.3 compatible functions
            try
            {
                GL.LoadEntryPoints();
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions. " +
                    "Try updating your graphics drivers.");
            }

        }

        public void MakeCurrent(IntPtr winHandle)
        {
            Sdl.GL.MakeCurrent(winHandle, _glContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                GraphicsDevice.DisposeContext(_glContext);
                _glContext = IntPtr.Zero;

                base.Dispose(disposing);
            }
        }

    }
}
