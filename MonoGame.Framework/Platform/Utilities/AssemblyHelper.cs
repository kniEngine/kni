// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Reflection;

namespace MonoGame.Framework.Utilities
{
    internal static class AssemblyHelper
    {
        public static string GetDefaultWindowTitle()
        {
            // When running NUnit tests entry assembly can return null.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                // Use the Title attribute of the Assembly if possible.
                try
                {
                    AssemblyTitleAttribute assemblyTitleAttrib = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute)));
                    if (assemblyTitleAttrib != null)
                    {
                        if (!string.IsNullOrEmpty(assemblyTitleAttrib.Title))
                            return assemblyTitleAttrib.Title;
                    }
                }
                catch { /* Nope, wasn't possible */ }

                // Otherwise, fallback to the Name of the assembly.
                return entryAssembly.GetName().Name;
            }

            return string.Empty;
        }
    }
}
