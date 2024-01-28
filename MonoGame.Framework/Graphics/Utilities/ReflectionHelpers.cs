// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Platform.Graphics.Utilities
{
    public static class ReflectionHelpers
    {
        public static bool IsValueType(Type targetType)
        {
#if WINRT
            return targetType.GetTypeInfo().IsValueType;
#else
            return targetType.IsValueType;
#endif
        }

        /// <summary>
        /// Returns the Assembly of a Type
        /// </summary>
        public static Assembly GetAssembly(Type targetType)
        {
#if WINRT
            return targetType.GetTypeInfo().Assembly;
#else
            return targetType.Assembly;
#endif
        }

        /// <summary>
        /// Returns true if the given type can be assigned the given value
        /// </summary>
        public static bool IsAssignableFrom(Type type, object value)
        {
            return IsAssignableFromType(type, value.GetType());
        }

        /// <summary>
        /// Returns true if the given type can be assigned a value with the given object type
        /// </summary>
        public static bool IsAssignableFromType(Type type, Type objectType)
        {
#if WINRT
            return (type.GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()));
#else
            return (type.IsAssignableFrom(objectType));
#endif
        }

        internal static TDelegate GetDelegateForFunctionPointer<TDelegate>(IntPtr ptr)
        {
#if NET40 || NET45 || NET40_OR_GREATER
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(TDelegate));
#else
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
#endif
        }

        public static int SizeOf<T>()
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
