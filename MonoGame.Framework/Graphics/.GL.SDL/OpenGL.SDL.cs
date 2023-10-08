// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal sealed class OGL_SDL : OGL
    {
        public static void Initialize()
        {
            System.Diagnostics.Debug.Assert(OGL._current == null);
            OGL._current = new OGL_SDL();
        }

        private OGL_SDL() : base()
        {
        }


        protected override IntPtr GetNativeLibrary()
        {
            BoundApi = RenderApi.GL;

            return IntPtr.Zero;
        }

        protected override T LoadFunction<T>(string function)
        {
            IntPtr funcAddress = Sdl.Current.OpenGL.GetProcAddress(function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            throw new EntryPointNotFoundException(function);
        }

        protected override T LoadFunctionOrNull<T>(string function)
        {
            IntPtr funcAddress = Sdl.Current.OpenGL.GetProcAddress(function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            return default(T);
        }

    }
}

