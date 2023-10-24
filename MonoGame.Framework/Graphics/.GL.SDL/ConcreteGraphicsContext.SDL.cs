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

        internal DrawBuffersEnum[] _drawBuffers;

        internal IntPtr GlContext { get { return _glContext; } }

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContext = SDL.OpenGL.CreateGLContext(context.DeviceStrategy.PresentationParameters.DeviceWindowHandle);

            // GL entry points must be loaded after the GL context creation, otherwise some Windows drivers will return only GL 1.3 compatible functions
            try
            {
                OGL_SDL.Initialize();
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions. " +
                    "Try updating your graphics drivers.");
            }

            MakeCurrent(context.DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(context.DeviceStrategy.PresentationParameters.PresentationInterval);
            SDL.OpenGL.SetSwapInterval(swapInterval);
        }

        public void MakeCurrent(IntPtr winHandle)
        {
            SDL.OpenGL.MakeCurrent(winHandle, _glContext);
        }

        public override void BindDisposeContext()
        {
            Microsoft.Xna.Framework.Threading.EnsureUIThread();
        }

        public override void UnbindDisposeContext()
        {
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

            if (_glContext != IntPtr.Zero)
                SDL.OpenGL.DeleteContext(_glContext);
            _glContext = IntPtr.Zero;

            base.Dispose(disposing);
        }

    }
}
