using System;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace Microsoft.Xna.Platform.Utilities
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
                return InteropHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            throw new EntryPointNotFoundException(function);
        }

        public static T LoadFunctionOrNull<T>(IntPtr library, string function)
        {
            IntPtr funcAddress = Dlfcn.dlsym(library, function);

            if (funcAddress != IntPtr.Zero)
                return InteropHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            return default(T);
        }
    }
}
