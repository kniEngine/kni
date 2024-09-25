// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;
using OpenGLES;

namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal sealed class OGL_IOS : OGL
	{
        public IntPtr Library = FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenGLES.framework/OpenGLES");

        public static void Initialize()
        {
            System.Diagnostics.Debug.Assert(OGL._current == null);
            OGL._current = new OGL_IOS();
        }

        private OGL_IOS() : base()
        {
        }


        protected override IntPtr GetNativeLibrary()
		{
			BoundApi = RenderApi.ES;

            return IntPtr.Zero;
        }

        protected override T LoadFunction<T>(string function)
        {
            return FuncLoader.LoadFunction<T>(Library, function);
        }

        protected override T LoadFunctionOrNull<T>(string function)
        {
            return FuncLoader.LoadFunctionOrNull<T>(Library, function);
        }

    }
}

