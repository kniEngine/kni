// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities
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

        public static Type GetBaseType(Type targetType)
        {
#if WINRT
            return targetType.GetTypeInfo().BaseType;
#else
            return targetType.BaseType;
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
        /// Returns true if the given type represents a non-object type that is not abstract.
        /// </summary>
        public static bool IsConcreteClass(Type t)
        {
            if (t == typeof(object))
                return false;
#if WINRT
            var ti = t.GetTypeInfo();
            return (ti.IsClass && !ti.IsAbstract);
#else            
            return (t.IsClass && !t.IsAbstract);
#endif
        }

        public static MethodInfo GetMethodInfo(Type type, string methodName)
        {
#if WINRT
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
#else
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        }

        public static MethodInfo GetPropertyGetMethod(PropertyInfo property)
        {
#if WINRT
            return property.GetMethod;
#else
            return property.GetGetMethod();
#endif
        }

        public static MethodInfo GetPropertySetMethod(PropertyInfo property)
        {
#if WINRT
            return property.SetMethod;
#else
            return property.GetSetMethod();
#endif
        }

        public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
#if WINRT
            return member.GetCustomAttribute(typeof(T)) as T;
#else
            return Attribute.GetCustomAttribute(member, typeof(T)) as T;
#endif
        }

        /// <summary>
        /// Returns true if the get method of the given property exist and are public.
        /// Note that we allow a getter-only property to be serialized (and deserialized),
        /// *if* CanDeserializeIntoExistingObject is true for the property type.
        /// </summary>
        public static bool PropertyIsPublic(PropertyInfo property)
        {
            var getMethod = GetPropertyGetMethod(property);

            return (getMethod != null && getMethod.IsPublic);
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
