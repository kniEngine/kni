﻿// MonoGame - Copyright (C) The MonoGame Team
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

        private static T LoadFunction<T>(string function)
        {
            IntPtr funcAddress = Sdl.GL.GetProcAddress(function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            throw new EntryPointNotFoundException(function);
        }

        private static T LoadFunctionOrNull<T>(string function)
        {
            IntPtr funcAddress = Sdl.GL.GetProcAddress(function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            return default(T);
        }

    }
}

