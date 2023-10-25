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
        OpenGLES.EAGLContext _glSharedContext;

        internal ConcreteGraphicsContext(GraphicsContext context)
            : base(context)
        {
            _glContextCurrentThreadId = Thread.CurrentThread.ManagedThreadId;

            iOSGameWindow gameWindow = iOSGameWindow.FromHandle(context.DeviceStrategy.PresentationParameters.DeviceWindowHandle);
            // TODO: Remove Game.Instance. iOSGameViewController/iOSGameView should be part of iOSGameWindow.
            ConcreteGame concreteGame = ConcreteGame.ConcreteGameInstance;
            iOSGameViewController viewController = concreteGame.ViewController;
            iOSGameView view = viewController.View;
            GLGraphicsContext glContext = viewController.View._glContext;
            
            try
            {
                _glSharedContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES3, glContext.Context.ShareGroup);
            }
            catch
            {
                // Fall back to GLES 2.0
                _glSharedContext = new OpenGLES.EAGLContext(OpenGLES.EAGLRenderingAPI.OpenGLES2, glContext.Context.ShareGroup);
            }

        }

        public override void BindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            OpenGLES.EAGLContext.SetCurrentContext(_glSharedContext);
        }

        public override void UnbindDisposeContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == _glContextCurrentThreadId)
                return;

            OpenGLES.EAGLContext.SetCurrentContext(null);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThrowIfDisposed();

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
