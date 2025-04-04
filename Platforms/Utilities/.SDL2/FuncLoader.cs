using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Platform.Utilities
{
    internal class FuncLoader
    {
        private class Windows
        {
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string lpszLib);
        }

        private class Linux
        {
            [DllImport("libdl.so.2")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("libdl.so.2")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        private class OSX
        {
            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlopen(string path, int flags);

            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }
        
        private const int RTLD_LAZY = 0x0001;

        public static IntPtr LoadLibraryExt(string libname)
        {
            IntPtr ret = IntPtr.Zero;

#if NET40 || NET45 || NET40_OR_GREATER
            string appDirectory = typeof(FuncLoader).Assembly.Location;
#else // NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
            string appDirectory = AppContext.BaseDirectory;
#endif
            appDirectory = Path.GetDirectoryName(appDirectory);
            appDirectory = appDirectory ?? "./";

            // Try .NET Framework / mono locations
            if (CurrentPlatform.OS == OS.MacOSX)
            {
                ret = LoadLibrary(Path.Combine(appDirectory, libname));

                // Look in Frameworks for .app bundles
                if (ret == IntPtr.Zero)
                    ret = LoadLibrary(Path.Combine(appDirectory, "..", "Frameworks", libname));
            }
            else
            {
                if (Environment.Is64BitProcess)
                    ret = LoadLibrary(Path.Combine(appDirectory, "x64", libname));
                else
                    ret = LoadLibrary(Path.Combine(appDirectory, "x86", libname));
            }

            // Try .NET Core development locations
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(appDirectory, "runtimes", CurrentPlatform.Rid, "native", libname));

            // Try current folder (.NET Core will copy it there after publish)
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(appDirectory, libname));

            // Try alternate way of checking current folder
            // assemblyLocation is null if we are inside macOS app bundle
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, libname));

            // Try loading system library
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(libname);

            // Welp, all failed, PANIC!!!
            if (ret == IntPtr.Zero)
                throw new Exception("Failed to load library: " + libname);

            return ret;
        }

        public static IntPtr LoadLibrary(string libname)
        {
            if (CurrentPlatform.OS == OS.Windows)
                return Windows.LoadLibraryW(libname);

            if (CurrentPlatform.OS == OS.MacOSX)
                return OSX.dlopen(libname, RTLD_LAZY);

            return Linux.dlopen(libname, RTLD_LAZY);
        }

        public static T LoadFunction<T>(IntPtr library, string function)
        {
            IntPtr funcAddress = IntPtr.Zero;

            if (CurrentPlatform.OS == OS.Windows)
                funcAddress = Windows.GetProcAddress(library, function);
            else if (CurrentPlatform.OS == OS.MacOSX)
                funcAddress = OSX.dlsym(library, function);
            else
                funcAddress = Linux.dlsym(library, function);

            if (funcAddress != IntPtr.Zero)
                return InteropHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            throw new EntryPointNotFoundException("Entry point not found for function '" + function + "'.");
        }

        public static T LoadFunctionOrNull<T>(IntPtr library, string function)
        {
            IntPtr funcAddress = IntPtr.Zero;

            if (CurrentPlatform.OS == OS.Windows)
                funcAddress = Windows.GetProcAddress(library, function);
            else if (CurrentPlatform.OS == OS.MacOSX)
                funcAddress = OSX.dlsym(library, function);
            else
                funcAddress = Linux.dlsym(library, function);

            if (funcAddress != IntPtr.Zero)
                return InteropHelpers.GetDelegateForFunctionPointer<T>(funcAddress);

            return default(T);
        }
    }
}
