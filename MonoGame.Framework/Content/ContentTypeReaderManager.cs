// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentTypeReaderManager
    {
        private static readonly object _locker;

        private static readonly Dictionary<String, Type> _contentTypeReadersCache;
        private static readonly Dictionary<Type, ContentTypeReader> _contentReadersCache;

        private Dictionary<Type, ContentTypeReader> _contentReaders;

        private static readonly string _contentAssemblyName;
        private static readonly string _contentGraphicsAssemblyName;
        private static readonly string _contentVideoAssemblyName;

        private static readonly bool _isRunningOnNetCore;

        static ContentTypeReaderManager()
        {
            _locker = new object();
            _contentTypeReadersCache = new Dictionary<string, Type>();
            _contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
            _contentAssemblyName = ReflectionHelpers.GetAssembly(typeof(ContentTypeReaderManager)).FullName;
            _contentGraphicsAssemblyName = _contentAssemblyName;
            _contentVideoAssemblyName = _contentAssemblyName;

            _isRunningOnNetCore = ReflectionHelpers.GetAssembly(typeof(System.Object)).GetName().Name == "System.Private.CoreLib";

        }

        public ContentTypeReader GetTypeReader(Type targetType)
        {
            if (targetType.IsArray && targetType.GetArrayRank() > 1)
                targetType = typeof(Array);

            ContentTypeReader reader;
            if (_contentReaders.TryGetValue(targetType, out reader))
                return reader;

            return null;
        }

        internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
        {
            PreserveContentTypeReaders();
            Microsoft.Xna.Framework.Audio.AudioService.PreserveAudioContentTypeReaders();

            // The first content byte i read tells me the number of content readers in this XNB file
            int numberOfReaders = reader.Read7BitEncodedInt();
            ContentTypeReader[] contentReaders = new ContentTypeReader[numberOfReaders];
            BitArray needsInitialize = new BitArray(numberOfReaders);
            _contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);

            // Lock until we're done allocating and initializing any new
            // content type readers...  this ensures we can load content
            // from multiple threads and still cache the readers.
            lock (_locker)
            {
                // For each reader in the file, we read out the length of the string which contains the type of the reader,
                // then we read out the string. Finally we instantiate an instance of that reader using reflection
                for (int i = 0; i < numberOfReaders; i++)
                {
                    // This string tells us what reader we need to decode the following data
                    string readerTypeName = reader.ReadString();
                    int readerTypeVersion = reader.ReadInt32();

                    Type typeReaderType;
                    if (!_contentTypeReadersCache.TryGetValue(readerTypeName, out typeReaderType))
                    {
                        typeReaderType = ResolveReaderType(readerTypeName);
                        _contentTypeReadersCache.Add(readerTypeName, typeReaderType);
                    }

                    ContentTypeReader typeReader;
                    if (!_contentReadersCache.TryGetValue(typeReaderType, out typeReader))
                    {
                        typeReader = typeReaderType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
                        needsInitialize[i] = true;
                        _contentReadersCache.Add(typeReaderType, typeReader);
                    }

                    if (readerTypeVersion != typeReader.TypeVersion)
                    {
                        throw new ContentLoadException(
                            String.Format("{0} of TypeVersion {1} does not match reader of TypeVersion {2}.",
                                typeReader.TargetType.Name, readerTypeVersion, typeReader.TypeVersion));
                    }

                    contentReaders[i] = typeReader;


                    Type targetType = contentReaders[i].TargetType;
                    if (targetType != null)
                        if (!_contentReaders.ContainsKey(targetType))
                            _contentReaders.Add(targetType, contentReaders[i]);
                }

                // Initialize any new readers.
                for (int i = 0; i < contentReaders.Length; i++)
                {
                    if (needsInitialize.Get(i))
                        contentReaders[i].Initialize(this);
                }

            } // lock (_locker)

            return contentReaders;
        }

        // Trick to prevent the linker removing the code, but not actually execute the code
        static bool _trimmingFalseFlag = false;
        private static void PreserveContentTypeReaders()
        {
#pragma warning disable 0219, 0649
            // Trick to prevent the linker removing the code, but not actually execute the code
            if (_trimmingFalseFlag)
            {
                // Dummy variables required for it to work with trimming ** DO NOT DELETE **
                // This forces the classes not to be optimized out when deploying with trimming

                // System types
                var hBooleanReader = new BooleanReader();
                var hByteReader = new ByteReader();
                var hCharReader = new CharReader();
                var hDateTimeReader = new DateTimeReader();
                var hDecimalReader = new DecimalReader();
                var hDoubleReader = new DoubleReader();
                var hInt16Reader = new Int16Reader();
                var hInt32Reader = new Int32Reader();
                var hInt64Reader = new Int64Reader();
                var hSByteReader = new SByteReader();
                var hSingleReader = new SingleReader();
                var hStringReader = new StringReader();
                var TimeSpanReader = new TimeSpanReader();
                var hUInt16Reader = new UInt16Reader();
                var hUInt32Reader = new UInt32Reader();
                var hUInt64Reader = new UInt64Reader();
                var hCharListReader = new ListReader<Char>();
                var hIntListReader = new ListReader<Int32>();
                var hArrayFloatReader = new ArrayReader<Single>();
                var hStringListReader = new ListReader<StringReader>();

                // Framework types
                var hBoundingBoxReader = new BoundingBoxReader();
                var hBoundingFrustumReader = new BoundingFrustumReader();
                var hBoundingSphereReader = new BoundingSphereReader();
                var hComplexReader = new ComplexReader();
                var hCurveReader = new CurveReader();
                var hExternalReferenceReader = new ExternalReferenceReader();
                var hMatrixReader = new MatrixReader();
                var hPlaneReader = new PlaneReader();
                var hPointReader = new PointReader();
                var hQuaternionReader = new QuaternionReader();
                var hRayReader = new RayReader();
                var hRectangleReader = new RectangleReader();
                var hVector2Reader = new Vector2Reader();
                var hVector3Reader = new Vector3Reader();
                var hVector4Reader = new Vector4Reader();
                var hArrayMatrixReader = new ArrayReader<Matrix>();
                var hRectangleArrayReader = new ArrayReader<Rectangle>();
                var hArrayVector2Reader = new ArrayReader<Vector2>();
                var hRectangleListReader = new ListReader<Rectangle>();
                var hVector3ListReader = new ListReader<Vector3>();
                var hListVector2Reader = new ListReader<Vector2>();
                var hNullableRectReader = new NullableReader<Rectangle>();

                // Framework.Graphics.PackedVector types
                var hColorReader = new ColorReader();
            }
#pragma warning restore 0219, 0649
        }

        /// <summary>
        /// Removes Version, Culture and PublicKeyToken from a type string.
        /// </summary>
        /// <remarks>
        /// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;) and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
        /// </remarks>
        /// <param name="readerTypeName">A <see cref="System.String"/></param>
        /// <returns>A <see cref="System.Type"/></returns>
        internal static Type ResolveReaderType(string readerTypeName)
        {
            // Handle nested types
            int count = readerTypeName.Split(new[] { "[[" }, StringSplitOptions.None).Length - 1;
            for (int i = 0; i < count; i++)
            {
                readerTypeName = Regex.Replace(readerTypeName, @"\[(.+?), Version=.+?\]", "[$1]");
            }

            // Handle non generic types
            if (readerTypeName.Contains("PublicKeyToken"))
                readerTypeName = Regex.Replace(readerTypeName, @"(.+?), Version=.+?$", "$1");

            string resolvedReaderTypeName;
            Type readerType;

            // map net.framework (.net4) to core.net (.net5 or later)
            if (readerTypeName.Contains(", mscorlib") && _isRunningOnNetCore)
            {
                resolvedReaderTypeName = readerTypeName;
                resolvedReaderTypeName = readerTypeName.Replace(", mscorlib", ", System.Private.CoreLib");
                readerType = Type.GetType(resolvedReaderTypeName);
                if (readerType != null)
                    return readerType;
            }
            // map core.net (.net5 or later) to net.framework (.net4) 
            if (readerTypeName.Contains(", System.Private.CoreLib") && !_isRunningOnNetCore)
            {
                resolvedReaderTypeName = readerTypeName;
                resolvedReaderTypeName = resolvedReaderTypeName.Replace(", System.Private.CoreLib", ", mscorlib");
                readerType = Type.GetType(resolvedReaderTypeName);
                if (readerType != null)
                    return readerType;
            }

            // map XNA build-in TypeReaders
            resolvedReaderTypeName = readerTypeName;
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", _contentGraphicsAssemblyName));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework.Video", string.Format(", {0}", _contentVideoAssemblyName));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", _contentAssemblyName));
            readerType = Type.GetType(resolvedReaderTypeName);
            if (readerType != null)
                return readerType;

            // map XNA & MonoGame build-in TypeReaders
            resolvedReaderTypeName = readerTypeName;
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", "Xna.Framework"));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", MonoGame.Framework", string.Format(", {0}", "Xna.Framework"));
            readerType = Type.GetType(resolvedReaderTypeName);
            if (readerType != null)
                return readerType;

            throw new ContentLoadException(
                    "Could not find ContentTypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: " +
                    readerTypeName + " (" + resolvedReaderTypeName + ")");
        }

    }
}
