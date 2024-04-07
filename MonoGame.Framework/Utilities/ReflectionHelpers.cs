// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Platform.Utilities
{
    internal static partial class ReflectionHelpers
    {
        internal static TDelegate GetDelegateForFunctionPointer<TDelegate>(IntPtr ptr)
        {
#if NET40 || NET45 || NET40_OR_GREATER
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(TDelegate));
#else
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
#endif
        }

    }
}
