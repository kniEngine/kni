// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;
using OpenGLES;

namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal partial class GL
	{
        public static IntPtr Library = FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenGLES.framework/OpenGLES");
        
        static partial void LoadPlatformEntryPoints()
		{
			BoundApi = RenderApi.ES;
        }

        private static T LoadFunction<T>(string function)
        {
            return FuncLoader.LoadFunction<T>(Library, function);
        }

        private static T LoadFunctionOrNull<T>(string function)
        {
            return FuncLoader.LoadFunctionOrNull<T>(Library, function);
        }

    }

    public class GLGraphicsContext : IDisposable
    {
        internal EAGLContext Context { get; private set; }

        public GLGraphicsContext()
        {
            try
            {
                Context = new EAGLContext(EAGLRenderingAPI.OpenGLES3);
            }
            catch
            {
                // Fall back to GLES 2.0
                Context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
            }
        }

        public bool IsCurrent { get { return EAGLContext.CurrentContext == this.Context; } }

        public void Dispose()
        {
            if (this.Context != null)
                this.Context.Dispose();

            this.Context = null;
        }

        public void MakeCurrent()
        {
            if (!EAGLContext.SetCurrentContext(this.Context))
            {
                throw new InvalidOperationException("Unable to change current EAGLContext.");
            }
        }

        public void SwapBuffers()
        {
            if (!this.Context.PresentRenderBuffer(36161u))
            {
                throw new InvalidOperationException("EAGLContext.PresentRenderbuffer failed.");
            }
        }

    }


    internal class GlesApi
    {
        public GlesApi()
        {
            GL.LoadEntryPoints();
        }

        public void BindFramebuffer(FramebufferTarget target, int framebuffer)
        {
            GL.BindFramebuffer(target, framebuffer);
        }

        public void BindRenderbuffer(RenderbufferTarget target, int renderbuffer)
        {
            GL.BindRenderbuffer(target, renderbuffer);
        }

        public void DeleteFramebuffer(int framebuffers)
        {
            GL.DeleteFramebuffer(framebuffers);
        }

        public void DeleteRenderbuffer(int renderbuffers)
        {
            GL.DeleteRenderbuffer(renderbuffers);
        }

        public void FramebufferRenderbuffer(
            FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, int renderbuffer)
        {
            GL.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
        }

        public int GenFramebuffer()
        {
            return GL.GenFramebuffer();
        }

        public int GenRenderbuffer()
        {
            return GL.GenRenderbuffer();
        }

        public void Scissor(int x, int y, int width, int height)
        {
            GL.Scissor(x, y, width, height);
        }

        public void Viewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }
    }
}

