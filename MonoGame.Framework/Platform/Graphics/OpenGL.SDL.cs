// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace MonoGame.OpenGL
{
    partial class GL
    {
        static partial void LoadPlatformEntryPoints()
        {
            BoundApi = RenderApi.GL;
        }

        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            var ret = Sdl.GL.GetProcAddress(function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default(T);
            }

            return ReflectionHelpers.GetDelegateForFunctionPointer<T>(ret);
        }

    }
    
    internal class GLGraphicsContext : IDisposable
    {
        private IntPtr _winHandle;
        private IntPtr _context;

        public GLGraphicsContext(IntPtr winHandle)
        {
            _winHandle = winHandle;
            _context = Sdl.GL.CreateGLContext(winHandle);

            // GL entry points must be loaded after the GL context creation, otherwise some Windows drivers will return only GL 1.3 compatible functions
            try
            {
                OpenGL.GL.LoadEntryPoints();
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
            _winHandle = winHandle;
            Sdl.GL.MakeCurrent(winHandle, _context);
        }

        public void SwapBuffers()
        {
            Sdl.GL.SwapWindow(_winHandle);
        }

        public void Dispose()
        {
            GraphicsDevice.DisposeContext(_context);
            _context = IntPtr.Zero;
            _winHandle = IntPtr.Zero;
        }
    }
}

