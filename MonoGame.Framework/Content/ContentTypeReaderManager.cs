// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentTypeReaderManager
    {
        private static readonly object _locker;

        private static readonly Dictionary<Type, ContentTypeReader> _contentReadersCache;

        private Dictionary<Type, ContentTypeReader> _contentReaders;

        private static readonly string _assemblyName;

        private static readonly bool _isRunningOnNetCore;

        static ContentTypeReaderManager()
        {
            _locker = new object();
            _contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
            _assemblyName = ReflectionHelpers.GetAssembly(typeof(ContentTypeReaderManager)).FullName;

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

        // Trick to prevent the linker removing the code, but not actually execute the code
        static bool falseflag = false;

        internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
        {
#pragma warning disable 0219, 0649
            // Trick to prevent the linker removing the code, but not actually execute the code
            if (falseflag)
            {
                // Dummy variables required for it to work on iDevices ** DO NOT DELETE ** 
                // This forces the classes not to be optimized out when deploying to iDevices
                var hByteReader = new ByteReader();
                var hSByteReader = new SByteReader();
                var hDateTimeReader = new DateTimeReader();
                var hDecimalReader = new DecimalReader();
                var hBoundingSphereReader = new BoundingSphereReader();
                var hBoundingFrustumReader = new BoundingFrustumReader();
                var hRayReader = new RayReader();
                var hCharListReader = new ListReader<Char>();
                var hRectangleListReader = new ListReader<Rectangle>();
                var hRectangleArrayReader = new ArrayReader<Rectangle>();
                var hVector3ListReader = new ListReader<Vector3>();
                var hStringListReader = new ListReader<StringReader>();
                var hIntListReader = new ListReader<Int32>();
                var hSpriteFontReader = new SpriteFontReader();
                var hTexture2DReader = new Texture2DReader();
                var hCharReader = new CharReader();
                var hRectangleReader = new RectangleReader();
                var hStringReader = new StringReader();
                var hVector2Reader = new Vector2Reader();
                var hVector3Reader = new Vector3Reader();
                var hVector4Reader = new Vector4Reader();
                var hCurveReader = new CurveReader();
                var hIndexBufferReader = new IndexBufferReader();
                var hBoundingBoxReader = new BoundingBoxReader();
                var hMatrixReader = new MatrixReader();
                var hBasicEffectReader = new BasicEffectReader();
                var hVertexBufferReader = new VertexBufferReader();
                var hAlphaTestEffectReader = new AlphaTestEffectReader();
                var hEnumSpriteEffectsReader = new EnumReader<Graphics.SpriteEffects>();
                var hArrayFloatReader = new ArrayReader<float>();
                var hArrayVector2Reader = new ArrayReader<Vector2>();
                var hListVector2Reader = new ListReader<Vector2>();
                var hArrayMatrixReader = new ArrayReader<Matrix>();
                var hEnumBlendReader = new EnumReader<Graphics.Blend>();
                var hNullableRectReader = new NullableReader<Rectangle>();
                var hEffectMaterialReader = new EffectMaterialReader();
                var hExternalReferenceReader = new ExternalReferenceReader();
                var hSoundEffectReader = new SoundEffectReader();
                var hSongReader = new SongReader();
                var hModelReader = new ModelReader();
                var hInt32Reader = new Int32Reader();
                var hEffectReader = new EffectReader();
                var hSingleReader = new SingleReader();

                // At the moment the Video class doesn't exist
                // on all platforms... Allow it to compile anyway.
#if ANDROID || IOS || MONOMAC || (WINDOWS && !OPENGL) || WINDOWS_UAP
                var hVideoReader = new VideoReader();
#endif
            }
#pragma warning restore 0219, 0649

            // The first content byte i read tells me the number of content readers in this XNB file
            var numberOfReaders = reader.Read7BitEncodedInt();
            var contentReaders = new ContentTypeReader[numberOfReaders];
            var needsInitialize = new BitArray(numberOfReaders);
            _contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);

            // Lock until we're done allocating and initializing any new
            // content type readers...  this ensures we can load content
            // from multiple threads and still cache the readers.
            lock (_locker)
            {
                // For each reader in the file, we read out the length of the string which contains the type of the reader,
                // then we read out the string. Finally we instantiate an instance of that reader using reflection
                for (var i = 0; i < numberOfReaders; i++)
                {
                    // This string tells us what reader we need to decode the following data
                    string readerTypeName = reader.ReadString();
                    // readerVersion is always zero
                    var readerVersion = reader.ReadInt32();

                    string resolvedReaderTypeName;
                    Type l_readerType = ResolveType(readerTypeName, out resolvedReaderTypeName);

                    if (l_readerType == null)
                    {
                        throw new ContentLoadException(
                                "Could not find ContentTypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: " +
                                readerTypeName + " (" + resolvedReaderTypeName + ")");
                    }

                    ContentTypeReader typeReader;
                    if (!_contentReadersCache.TryGetValue(l_readerType, out typeReader))
                    {
                        typeReader = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
                        needsInitialize[i] = true;
                        _contentReadersCache.Add(l_readerType, typeReader);
                    }

                    contentReaders[i] = typeReader;


                    var targetType = contentReaders[i].TargetType;
                    if (targetType != null)
                        if (!_contentReaders.ContainsKey(targetType))
                            _contentReaders.Add(targetType, contentReaders[i]);
                }

                // Initialize any new readers.
                for (var i = 0; i < contentReaders.Length; i++)
                {
                    if (needsInitialize.Get(i))
                        contentReaders[i].Initialize(this);
                }

            } // lock (_locker)

            return contentReaders;
        }

        /// <summary>
        /// Removes Version, Culture and PublicKeyToken from a type string.
        /// </summary>
        /// <remarks>
        /// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;) and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
        /// </remarks>
        /// <param name="readerTypeName">
        /// A <see cref="System.String"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Type"/>
        /// </returns>
        internal static Type ResolveType(string readerTypeName, out string resolvedReaderTypeName)
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

            // map net.framework (.net4) to core.net (.net5 or later)
            if (readerTypeName.Contains(", mscorlib") && _isRunningOnNetCore)
            {
                resolvedReaderTypeName = readerTypeName.Replace(", mscorlib", ", System.Private.CoreLib");
                return Type.GetType(resolvedReaderTypeName);
            }
            // map core.net (.net5 or later) to net.framework (.net4) 
            if (readerTypeName.Contains(", System.Private.CoreLib") && !_isRunningOnNetCore)
            {
                resolvedReaderTypeName = readerTypeName.Replace(", System.Private.CoreLib", ", mscorlib");
                return Type.GetType(resolvedReaderTypeName);
            }

            // map XNA build-in TypeReaders
            resolvedReaderTypeName = readerTypeName;
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", _assemblyName));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework.Video", string.Format(", {0}", _assemblyName));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", _assemblyName));
            Type resolvedType = Type.GetType(resolvedReaderTypeName);
            if (resolvedType != null)
                return resolvedType;

            // map XNA & Monogame build-in TypeReaders
            resolvedReaderTypeName = readerTypeName;
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", "Xna.Framework"));
            resolvedReaderTypeName = resolvedReaderTypeName.Replace(", MonoGame.Framework", string.Format(", {0}", "Xna.Framework"));
            resolvedType = Type.GetType(resolvedReaderTypeName);
            if (resolvedType != null)
                return resolvedType;

            return null;
        }

    }
}
