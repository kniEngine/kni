// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Android.Opengl;
using Android.Runtime;
using Javax.Microedition.Khronos.Egl;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal sealed class OGL_DROID : OGL
    {
		// internal for Android is not used on other platforms
		// it allows us to use either GLES or Full GL (if the GPU supports it)
		internal delegate bool BindAPIDelegate(RenderApi api);
		internal BindAPIDelegate BindAPI;

        public IntPtr Library;
        public IntPtr libES1 = FuncLoader.LoadLibrary("libGLESv1_CM.so");
        public IntPtr libES2 = FuncLoader.LoadLibrary("libGLESv2.so");
        public IntPtr libES3 = FuncLoader.LoadLibrary("libGLESv3.so");
        public IntPtr libGL = FuncLoader.LoadLibrary("libGL.so");

        private IEGL10 _egl;

        public IEGL10 Egl { get { return _egl; } }

        public static void Initialize()
        {
            System.Diagnostics.Debug.Assert(OGL._current == null);
            OGL._current = new OGL_DROID();

        }

        private OGL_DROID() : base()
        {
            _egl = Javax.Microedition.Khronos.Egl.EGLContext.EGL.JavaCast<IEGL10>();
        }


        protected override IntPtr GetNativeLibrary()
        {
            Android.Util.Log.Verbose("GL", "Loading Entry Points");

            BindAPI = FuncLoader.LoadFunctionOrNull<BindAPIDelegate>(libGL, "eglBindAPI");

            if (BindAPI != null)
            {
                if (BindAPI(RenderApi.GL))
                    BoundApi = RenderApi.GL;
                else if (BindAPI(RenderApi.ES))
                    BoundApi = RenderApi.ES;
                else
                    BoundApi = RenderApi.ES;
            }
            else
            {
                BoundApi = RenderApi.ES;
            }
                
            Android.Util.Log.Verbose("GL", "Bound {0}", BoundApi);

            if (BoundApi == RenderApi.ES)
            {
                if (libES3 != IntPtr.Zero)
                    Library = libES3;
                if (libES2 != IntPtr.Zero)
                    Library = libES2;
            }
            else if (BoundApi == RenderApi.GL)
            {
                if (libGL != IntPtr.Zero)
                    Library = libGL;
            }

            return Library;
        }

        protected override T LoadFunction<T>(string function)
        {
            return FuncLoader.LoadFunction<T>(Library, function);
        }

        protected override T LoadFunctionOrNull<T>(string function)
        {
            return FuncLoader.LoadFunctionOrNull<T>(Library, function);
        }

        internal IEnumerable<GLESVersion> GetSupportedGLESVersions()
        {
            if (this.libES3 != IntPtr.Zero)
            {
                yield return new GLESVersion { Major = 3, Minor = 2 };
                yield return new GLESVersion { Major = 3, Minor = 1 };
                yield return new GLESVersion { Major = 3, Minor = 0 };
            }
            if (this.libES2 != IntPtr.Zero)
            {
                // We pass -1 because when requesting a GLES 2.0 context we
                // dont provide the Minor version.
                yield return new GLESVersion { Major = 2, Minor = -1 };
            }
            yield return new GLESVersion();
        }

    }

    struct GLESVersion
    {
        const int EglContextClientVersion = 0x3098;
        const int EglContextMinorVersion = 0x30fb;

        public int Major;
        public int Minor;

        internal int[] GetAttributes()
        {
            int minor = Minor > -1 ? EglContextMinorVersion : EGL10.EglNone;
            return new int[] { EglContextClientVersion, Major, minor, Minor, EGL10.EglNone };
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Major, Minor == -1 ? 0 : Minor);
        }

    }
}
