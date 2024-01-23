// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{
    internal static class ContentExtensions
    {
        public static FieldInfo[] GetAllFields(this Type type)
        {
#if WINRT
            FieldInfo[] fields= type.GetTypeInfo().DeclaredFields.ToArray();
            IEnumerable<FieldInfo> nonStaticFields = from field in fields
                    where !field.IsStatic
                    select field;
            return nonStaticFields.ToArray();
#else
            BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return type.GetFields(attrs);
#endif
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
        {

            // Sometimes, overridden properties of abstract classes can show up even with 
            // BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
            // all properties in this list are defined in this class by comparing
            // its get method with that of it's base class. If they're the same
            // Then it's an overridden property.
#if WINRT
            PropertyInfo[] infos= type.GetTypeInfo().DeclaredProperties.ToArray();
            IEnumerable<PropertyInfo> nonStaticPropertyInfos = from p in infos
                                         where (p.GetMethod != null) && (!p.GetMethod.IsStatic) &&
                                         (p.GetMethod == p.GetMethod.GetRuntimeBaseDefinition())
                                         select p;
            return nonStaticPropertyInfos.ToArray();
#else
            const BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            List<PropertyInfo> allProps = type.GetProperties(attrs).ToList();
            PropertyInfo[] props = allProps.FindAll(p => p.GetGetMethod(true) != null && p.GetGetMethod(true) == p.GetGetMethod(true).GetBaseDefinition()).ToArray();
            return props;
#endif
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
#if WINRT
            TypeInfo typeInfo = type.GetTypeInfo();
            ConstructorInfo ctor = typeInfo.DeclaredConstructors.FirstOrDefault(c => !c.IsStatic && c.GetParameters().Length == 0);
            return ctor;
#else
            BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return type.GetConstructor(attrs, null, new Type[0], null);
#endif
        }

        public static bool IsClass(this Type type)
        {
#if WINRT
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }
    }
}
