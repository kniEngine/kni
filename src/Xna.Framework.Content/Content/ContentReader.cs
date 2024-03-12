// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentReader : BinaryReader
    {
        private ContentManager contentManager;
        private Action<IDisposable> recordDisposableObject;
        
        private ContentTypeReaderManager typeReaderManager;
        private string assetName;
        private List<KeyValuePair<int, Action<object>>> sharedResourceFixups;
        private ContentTypeReader[] typeReaders;
        internal int version;
        internal int xnbLength;
        internal int sharedResourceCount;

        internal ContentTypeReader[] TypeReaders
        {
            get { return typeReaders; }
        }

        internal ContentReader(ContentManager manager, Stream stream, string assetName, int version, int xnbLength, Action<IDisposable> recordDisposableObject)
            : base(stream)
        {
            this.recordDisposableObject = recordDisposableObject;
            this.contentManager = manager;
            this.assetName = assetName;
            this.version = version;
            this.xnbLength = xnbLength;
        }

        public ContentManager ContentManager
        {
            get { return contentManager; }
        }
        
        public string AssetName
        {
            get { return assetName; }
        }

        public ContentBufferPool BufferPool
        {
            get { return ContentBufferPool.Current; }
        }

        internal object ReadAsset<T>()
        {
            InitializeTypeReaders();

            // Read primary object
            T result = ReadObject<T>();

            // Read shared resources
            ReadSharedResources();
            
            return result;
        }

        internal void InitializeTypeReaders()
        {
            typeReaderManager = new ContentTypeReaderManager();
            typeReaders = typeReaderManager.LoadAssetReaders(this);
            sharedResourceCount = Read7BitEncodedInt();
            sharedResourceFixups = new List<KeyValuePair<int, Action<object>>>();
        }

        internal void ReadSharedResources()
        {
            if (sharedResourceCount <= 0)
                return;

            object[] sharedResources = new object[sharedResourceCount];
            for (int i = 0; i < sharedResourceCount; ++i)
            {
                object existingInstance;
                string key = assetName.Replace('\\', '/') +"_SharedResource_" + i + "_" + this.xnbLength;
                contentManager._loadedSharedResources.TryGetValue(key, out existingInstance);
                
                sharedResources[i] = ReadObject<object>(existingInstance);

                if (existingInstance == null)
                    contentManager._loadedSharedResources[key] = sharedResources[i];
            }

            // Fixup shared resources by calling each registered action
            for (int i = 0; i < sharedResourceFixups.Count; ++i)
            {
                KeyValuePair<int, Action<object>> fixup = sharedResourceFixups[i];
                object sharedResource = sharedResources[fixup.Key];
                Action<object> fixupAction = fixup.Value;
                fixupAction(sharedResource);
            }
        }

        public T ReadExternalReference<T>()
        {
            string externalReference = ReadString();

            if (!String.IsNullOrEmpty(externalReference))
            {
                return contentManager.Load<T>(FileHelpers.ResolveRelativePath(assetName, externalReference));
            }

            return default(T);
        }

            
        private void RecordDisposable<T>(T result)
        {
            IDisposable disposable = result as IDisposable;
            if (disposable == null)
                return;

            if (recordDisposableObject != null)
                recordDisposableObject(disposable);
            else
                contentManager.RecordDisposable(disposable);
        }

        public T ReadObject<T>()
        {
            return ReadObject<T>(default(T));
        }

        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            T result = (T)typeReader.Read(this, default(T));
            RecordDisposable(result);
            return result;
        }

        public T ReadObject<T>(T existingInstance)
        {
            int typeReaderIndex = Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;

            if (typeReaderIndex > typeReaders.Length)
                throw new ContentLoadException("Incorrect type reader index found!");

            ContentTypeReader typeReader = typeReaders[typeReaderIndex - 1];
            T result = (T)typeReader.Read(this, existingInstance);

            RecordDisposable(result);

            return result;
        }

        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            if (!ReflectionHelpers.IsValueType(typeReader.TargetType))
                return ReadObject(existingInstance);

            T result = (T)typeReader.Read(this, existingInstance);

            RecordDisposable(result);

            return result;
        }


        public T ReadRawObject<T>()
        {
            return (T)ReadRawObject<T> (default(T));
        }

        public T ReadRawObject<T>(ContentTypeReader typeReader)
        {
            return (T)ReadRawObject<T>(typeReader, default(T));
        }

        public T ReadRawObject<T>(T existingInstance)
        {
            Type objectType = typeof(T);
            foreach(ContentTypeReader typeReader in typeReaders)
            {
                if(typeReader.TargetType == objectType)
                    return (T)ReadRawObject<T>(typeReader,existingInstance);
            }
            throw new NotSupportedException();
        }

        public T ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            return (T)typeReader.Read(this, existingInstance);
        }

        public void ReadSharedResource<T>(Action<T> fixup)
        {
            int index = Read7BitEncodedInt();
            if (index > 0)
            {
                sharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(index - 1, delegate(object v)
                    {
                        if (!(v is T))
                        {
                            throw new ContentLoadException(String.Format("Error loading shared resource. Expected type {0}, received type {1}", typeof(T).Name, v.GetType().Name));
                        }
                        fixup((T)v);
                    }));
            }
        }

        internal new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        public Matrix ReadMatrix()
        {
            Matrix result;
            result.M11 = ReadSingle();
            result.M12 = ReadSingle();
            result.M13 = ReadSingle();
            result.M14 = ReadSingle();
            result.M21 = ReadSingle();
            result.M22 = ReadSingle();
            result.M23 = ReadSingle();
            result.M24 = ReadSingle();
            result.M31 = ReadSingle();
            result.M32 = ReadSingle();
            result.M33 = ReadSingle();
            result.M34 = ReadSingle();
            result.M41 = ReadSingle();
            result.M42 = ReadSingle();
            result.M43 = ReadSingle();
            result.M44 = ReadSingle();
            return result;
        }

        internal Complex ReadComplex()
        {
            Complex result;
            result.R = ReadSingle();
            result.i = ReadSingle();
            return result;
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion result;
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            result.W = ReadSingle();
            return result;
        }

        public Vector2 ReadVector2()
        {
            Vector2 result;
            result.X = ReadSingle();
            result.Y = ReadSingle();
            return result;
        }

        public Vector3 ReadVector3()
        {
            Vector3 result;
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            return result;
        }

        public Vector4 ReadVector4()
        {
            Vector4 result;
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            result.W = ReadSingle();
            return result;
        }

    }
}
