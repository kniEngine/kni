﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities.LZ4;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides methods for writing compiled binary format.
    /// </summary>
    public sealed class ContentCompiler
    {
        const byte XnbFormatVersion = 5;

        const byte ContentFlagCompressedLzx = 0x80;
        const byte ContentFlagCompressedLz4 = 0x40;
        const byte ContentFlagHiDef = 0x01;

        // This array must remain in sync with TargetPlatform
        static char[] _targetPlatformIdentifiers = new[]
        {
            'w', // Windows (DirectX)
            'x', // Xbox360
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'r', // RaspberryPi
            'P', // PlayStation4
            '5', // PlayStation5
            'O', // XboxOne
            'S', // Nintendo Switch
            'b', // BlazorGL
        };

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
            using (MemoryStream contentStream = new MemoryStream())
            using (ContentWriter contentwriter = new ContentWriter(this, contentStream, targetPlatform, targetProfile, rootDirectory, referenceRelocationPath))
            {
                contentwriter.WriteObject(content);
                contentwriter.WriteSharedResources(contentwriter.SharedResources);

                if (!compressContent)
                {
                    BufferedStream bufferedStream = new BufferedStream(stream);
                    WriteHeader(bufferedStream, targetPlatform, targetProfile, compressContent);
                    long fileSizePos = bufferedStream.Position;
                    bufferedStream.WriteByte(0); bufferedStream.WriteByte(0);
                    bufferedStream.WriteByte(0); bufferedStream.WriteByte(0);

                    using (XnbBodyWriter xnbBodyWriter = new XnbBodyWriter(bufferedStream))
                    {
                        xnbBodyWriter.WriteTypeWriters(contentwriter.TypeWriters, targetPlatform);
                        xnbBodyWriter.WriteSharedResourcesCount(contentwriter.SharedResources.Count);
                    }
                    contentStream.Position = 0;
                    contentStream.CopyTo(bufferedStream);


                    // Write the file size.
                    bufferedStream.Position = fileSizePos;
                    UInt32 fileSize = (uint)bufferedStream.Length;
                    WriteUInt(bufferedStream, (uint)bufferedStream.Length);
                    bufferedStream.Flush();
                }
                else
                {
                    MemoryStream bodyStream = new MemoryStream();
                    using (XnbBodyWriter xnbBodyWriter = new XnbBodyWriter(bodyStream))
                    {
                        xnbBodyWriter.WriteTypeWriters(contentwriter.TypeWriters, targetPlatform);
                        xnbBodyWriter.WriteSharedResourcesCount(contentwriter.SharedResources.Count);
                    }
                    contentStream.Position = 0;
                    contentStream.CopyTo(bodyStream);

                    // Before we write the header, try to compress the body stream. If compression fails, we want to
                    // turn off the compressContent flag so the correct flags are written in the header
                    Stream compressedStream = CompressStream(bodyStream);
                    if (compressedStream == null)
                    {
                        compressContent = false;
                        compressedStream = bodyStream;
                    }

                    BufferedStream bufferedStream = new BufferedStream(stream);
                    WriteHeader(bufferedStream, targetPlatform, targetProfile, compressContent);

                    long fileSizePos = bufferedStream.Position;
                    WriteUInt(bufferedStream, (uint)(fileSizePos + sizeof(UInt32) + compressedStream.Length));

                    compressedStream.Position = 0;
                    compressedStream.CopyTo(bufferedStream);
                    bufferedStream.Flush();
                }
            }

            return;
        }

        private static void WriteUInt(Stream bufferedStream, uint fileSize)
        {
            byte[] data = BitConverter.GetBytes(fileSize);
            bufferedStream.Write(data, 0, data.Length);
        }

        private MemoryStream CompressStream(MemoryStream bodyStream)
        {
            byte[] plainData = new byte[bodyStream.Length];
            bodyStream.Position = 0;
            bodyStream.Read(plainData, 0, plainData.Length);

            // Compress stream
            int maxLength = LZ4Codec.MaximumOutputLength((int)plainData.Length);
            byte[] outputArray = new byte[maxLength * 2];
            int resultLength = LZ4Codec.Encode32HC(plainData, 0, (int)plainData.Length, outputArray, 0, maxLength);
            if (resultLength < 0)
                return null;
            if (resultLength >= plainData.Length)
                return null;

            MemoryStream compressedStream = new MemoryStream();
            WriteUInt(compressedStream, (uint)bodyStream.Length);
            compressedStream.Write(outputArray, 0, resultLength);
            return compressedStream;
        }

        /// <summary>
        /// Write the header to the output stream.
        /// </summary>
        private void WriteHeader(Stream stream, TargetPlatform targetPlatform, GraphicsProfile targetProfile, bool compressContent)
        {
            stream.WriteByte((byte)'X');
            stream.WriteByte((byte)'N');
            stream.WriteByte((byte)'B');

            stream.WriteByte((byte)_targetPlatformIdentifiers[(int)targetPlatform]);
            stream.WriteByte(XnbFormatVersion);

            byte flags = default(byte);

            // We cannot use LZX compression, so we use the public domain LZ4 compression. Use one of the spare bits in the flags byte to specify LZ4.
            if (compressContent)
                flags |= ContentFlagCompressedLz4;

            if (targetProfile == GraphicsProfile.HiDef)
            {
                flags |= ContentFlagHiDef;
            }

            stream.WriteByte(flags);
        }

        private class XnbBodyWriter : BinaryWriter
        {
            public XnbBodyWriter(Stream output) : base(output)
            {
            }

            public void WriteTypeWriters(ICollection<ContentTypeWriter> typeWriters, TargetPlatform targetPlatform)
            {
                Write7BitEncodedInt(typeWriters.Count);
                foreach (ContentTypeWriter typeWriter in typeWriters)
                {
                    Write(typeWriter.GetRuntimeReader(targetPlatform));
                    Write(typeWriter.TypeVersion);
                }
            }

            public void WriteSharedResourcesCount(int sharedResourcesCount)
            {
                Write7BitEncodedInt(sharedResourcesCount);
            }

            /// <summary>
            /// Releases the resources used by the IDisposable class.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                }

                //base.Dispose(disposing);
            }
        }
    }
}
