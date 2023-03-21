// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides methods for writing compiled binary format.
    /// </summary>
    public sealed class ContentCompiler
    {
        readonly Dictionary<Type, Type> _typeWriterMap = new Dictionary<Type, Type>();

        /// <summary>
        /// Initializes a new instance of ContentCompiler.
        /// </summary>
        public ContentCompiler()
        {
            GetTypeWriters();
        }

        /// <summary>
        /// Iterates through all loaded assemblies and finds the content type writers.
        /// </summary>
        void GetTypeWriters()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (Exception)
                {
                    continue;
                }

                Type contentTypeWriterGenericType = typeof(ContentTypeWriter<>);

                foreach (Type type in exportedTypes)
                {
					if (type.IsAbstract)
                        continue;
                    if (!Attribute.IsDefined(type, typeof(ContentTypeWriterAttribute)))
                        continue;
                    
                    // Find the content type this writer implements
                    Type baseType = type.BaseType;
                    while ((baseType != null) && (baseType.GetGenericTypeDefinition() != contentTypeWriterGenericType))
                    {
                        baseType = baseType.BaseType;
                    }

                    if (baseType != null)
                        _typeWriterMap.Add(baseType, type);
                }
            }
        }

        /// <summary>
        /// Retrieves the worker writer for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The worker writer.</returns>
        public ContentTypeWriter GetTypeWriter(Type type)
        {
            try
            {
                Type writerType = GetTypeWriterType(type);
                ContentTypeWriter writer = (ContentTypeWriter)Activator.CreateInstance(writerType);
                writer.Initialize(this);

                return writer;
            }
            catch (Exception)
            {
                throw new InvalidContentException(String.Format("Could not find ContentTypeWriter for type '{0}'", type.Name));
            }
        }

        private Type GetTypeWriterType(Type type)
        {
            if (type == typeof(Array))
            {
                Type resultType = typeof(ArrayWriter<Array>);
                return resultType;
            }

            lock (_typeWriterMap)
            {
                Type contentTypeWriterGenericType = typeof(ContentTypeWriter<>);
                Type contentTypeWriterType = contentTypeWriterGenericType.MakeGenericType(type);

                Type typeWriterType;
                if (_typeWriterMap.TryGetValue(contentTypeWriterType, out typeWriterType))
                    return typeWriterType;

                if (type.IsArray)
                {
                    Type writerGenericType = (type.GetArrayRank() == 1)
                                           ? typeof(ArrayWriter<>)
                                           : typeof(MultiArrayWriter<>);

                    Type writerType = writerGenericType.MakeGenericType(type.GetElementType());
                    _typeWriterMap.Add(contentTypeWriterType, writerType);
                    return writerType;
                }

                if (type.IsEnum)
                {
                    Type writerGenericType = typeof(EnumWriter<>);
                    Type writerType = writerGenericType.MakeGenericType(type);
                    _typeWriterMap.Add(contentTypeWriterType, writerType);
                    return writerType;
                }

                if (type.IsGenericType)
                {
                    Type writerGenericType = null;

                    Type inputTypeDef = type.GetGenericTypeDefinition();
                    foreach (var kvp in _typeWriterMap)
                    {
                        Type[] args = kvp.Key.GetGenericArguments();

                        if (args.Length == 0)
                            continue;
                        if (!kvp.Value.IsGenericTypeDefinition)
                            continue;
                        if (!args[0].IsGenericType)
                            continue;

                        // Compare generic type definition
                        Type keyTypeDef = args[0].GetGenericTypeDefinition();
                        if (inputTypeDef == keyTypeDef)
                        {
                            writerGenericType = kvp.Value;
                            break;
                        }
                    }

                    if (writerGenericType != null)
                    {
                        Type[] args = type.GetGenericArguments();
                        Type writerType = writerGenericType.MakeGenericType(args);
                        _typeWriterMap.Add(contentTypeWriterType, writerType);
                        return writerType;
                    }
                }

                {
                    Type writerGenericType = typeof(ReflectiveWriter<>);
                    Type writerType = writerGenericType.MakeGenericType(type);
                    _typeWriterMap.Add(contentTypeWriterType, writerType);
                    return writerType;
                }
            }
        }

        /// <summary>
        /// Write the content to a XNB file.
        /// </summary>
        /// <param name="stream">The stream to write the XNB file to.</param>
        /// <param name="content">The content to write to the XNB file.</param>
        /// <param name="targetPlatform">The platform the XNB is intended for.</param>
        /// <param name="targetProfile">The graphics profile of the target.</param>
        /// <param name="compressContent">True if the content should be compressed.</param>
        /// <param name="rootDirectory">The root directory of the content.</param>
        /// <param name="referenceRelocationPath">The path of the XNB file, used to calculate relative paths for external references.</param>
        public void Compile(Stream stream, object content, TargetPlatform targetPlatform, GraphicsProfile targetProfile, bool compressContent, string rootDirectory, string referenceRelocationPath)
        {
            using (var writer = new ContentWriter(this, stream, targetPlatform, targetProfile, compressContent, rootDirectory, referenceRelocationPath))
            {
                writer.WriteObject(content);
                writer.Flush();
            }
        }
    }
}
