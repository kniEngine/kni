// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : ConcreteGraphicsContextGL
    {
        private GLGraphicsContext _glContext;

        internal GLGraphicsContext GlContext { get { return _glContext; } }

        internal ConcreteGraphicsContext(IntPtr sdlWindowHandle) : base()
        {
            var glContext = new GLGraphicsContext(sdlWindowHandle);
            _glContext = glContext;

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

                if (_glContext != null)
                    _glContext.Dispose();
                _glContext = null;

                base.Dispose();
            }
        }
    }
}
