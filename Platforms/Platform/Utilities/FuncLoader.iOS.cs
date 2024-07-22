using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Utilities;
using ObjCRuntime;

namespace MonoGame.Framework.Utilities
{
    internal class FuncLoader
    {
        public static IntPtr LoadLibrary(string libname)
        {
            return Dlfcn.dlopen(libname, 0);
        }

        public static T LoadFunction<T>(IntPtr library, string function)
        {
            IntPtr funcAddress = Dlfcn.dlsym(library, function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            throw new EntryPointNotFoundException(function);
        }

        public static T LoadFunctionOrNull<T>(IntPtr library, string function)
        {
            IntPtr funcAddress = Dlfcn.dlsym(library, function);

            if (funcAddress != IntPtr.Zero)
                return ReflectionHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            return default(T);
        }
    }
}
