// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides methods for writing compiled binary format.
    /// </summary>
    public sealed class ContentCompiler
    {
        readonly Dictionary<Type, Type> typeWriterMap = new Dictionary<Type, Type>();

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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
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

                var contentTypeWriterType = typeof(ContentTypeWriter<>);
                foreach (var type in exportedTypes)
                {
					if (type.IsAbstract)
                        continue;
                    if (Attribute.IsDefined(type, typeof(ContentTypeWriterAttribute)))
                    {
                        // Find the content type this writer implements
                        Type baseType = type.BaseType;
                        while ((baseType != null) && (baseType.GetGenericTypeDefinition() != contentTypeWriterType))
                            baseType = baseType.BaseType;
                        if (baseType != null)
                            typeWriterMap.Add(baseType, type);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the worker writer for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The worker writer.</returns>
        /// <remarks>This should be called from the ContentTypeWriter.Initialize method.</remarks>
        public ContentTypeWriter GetTypeWriter(Type type)
        {
            ContentTypeWriter result = CreateTypeWriter(type);
            result.Initialize(this);
            return result;
        }

        private ContentTypeWriter CreateTypeWriter(Type type)
        {
            if (type == typeof(Array))
                return new ArrayWriter<Array>();

            Type contentTypeWriterType = typeof(ContentTypeWriter<>).MakeGenericType(type);

            Type typeWriterType;
            if (typeWriterMap.TryGetValue(contentTypeWriterType, out typeWriterType))
                return (ContentTypeWriter)Activator.CreateInstance(typeWriterType);

            if (type.IsArray)
            {
                Type writerType = type.GetArrayRank() == 1 ? typeof(ArrayWriter<>) : typeof(MultiArrayWriter<>);

                Type genericType = writerType.MakeGenericType(type.GetElementType());
                ContentTypeWriter result = (ContentTypeWriter)Activator.CreateInstance(genericType);
                Type resultType = result.GetType();
                System.Diagnostics.Debug.Assert(genericType == resultType);
                typeWriterMap.Add(contentTypeWriterType, resultType);
                return result;
            }

            if (type.IsEnum)
            {
                Type genericType = typeof(EnumWriter<>).MakeGenericType(type);
                ContentTypeWriter result = (ContentTypeWriter)Activator.CreateInstance(genericType);
                Type resultType = result.GetType();
                System.Diagnostics.Debug.Assert(genericType == resultType);
                typeWriterMap.Add(contentTypeWriterType, resultType);
                return result;
            }

            if (type.IsGenericType)
            {
                Type inputTypeDef = type.GetGenericTypeDefinition();

                Type chosen = null;
                foreach (var kvp in typeWriterMap)
                {
                    var args = kvp.Key.GetGenericArguments();

                    if (args.Length == 0)
                        continue;

                    if (!kvp.Value.IsGenericTypeDefinition)
                        continue;

                    if (!args[0].IsGenericType)
                        continue;

                    // Compare generic type definition
                    var keyTypeDef = args[0].GetGenericTypeDefinition();
                    if (inputTypeDef == keyTypeDef)
                    {
                        chosen = kvp.Value;
                        break;
                    }
                }

                try
                {
                    Type genericType;
                    ContentTypeWriter result;
                    if (chosen == null)
                    {
                        genericType = typeof(ReflectiveWriter<>).MakeGenericType(type);
                        result = (ContentTypeWriter)Activator.CreateInstance(genericType);
                    }
                    else
                    {
                        var args = type.GetGenericArguments();
                        genericType = chosen.MakeGenericType(args);
                        result = (ContentTypeWriter)Activator.CreateInstance(genericType);
                    }

                    // save it for next time.
                    Type resultType = result.GetType();
                    System.Diagnostics.Debug.Assert(genericType == resultType);
                    typeWriterMap.Add(contentTypeWriterType, resultType);
                    return result;
                }
                catch (Exception)
                {
                    throw new InvalidContentException(String.Format("Could not find ContentTypeWriter for type '{0}'", type.Name));
                }
            }

            {
                Type genericType = typeof(ReflectiveWriter<>).MakeGenericType(type);
                ContentTypeWriter result = (ContentTypeWriter)Activator.CreateInstance(genericType);
                Type resultType = result.GetType();
                System.Diagnostics.Debug.Assert(genericType == resultType);
                typeWriterMap.Add(contentTypeWriterType, resultType);
                return result;
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
