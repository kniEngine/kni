// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Platform.Graphics.Utilities
{
    internal static partial class ReflectionHelpers
    {
        public static bool IsValueType(Type targetType)
        {
#if WINRT
            return targetType.GetTypeInfo().IsValueType;
#else
            return targetType.IsValueType;
#endif
        }

        internal static int SizeOf<T>()
        {
            return ManagedSizeOf<T>.Value;
        }

        /// <summary>
        /// Generics handler for Marshal.SizeOf
        /// </summary>
        private static class ManagedSizeOf<T>
        {
            static public int Value { get; private set; }

            static ManagedSizeOf()
            {
#if NET40 || NET45 || NET40_OR_GREATER
                Value = Marshal.SizeOf(typeof(T));
#else
                Value = Marshal.SizeOf<T>();
#endif
            }
        }
    }
}
