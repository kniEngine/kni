﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;
using ObjCRuntime;
using System.Security;
using OpenGLES;

namespace MonoGame.OpenGL
{
    internal partial class GL
	{
        public static IntPtr Library = FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenGLES.framework/OpenGLES");
        
        static partial void LoadPlatformEntryPoints()
		{
			BoundApi = RenderApi.ES;
        }

        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            return FuncLoader.LoadFunction<T>(Library, function, throwIfNotFound);
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
}

