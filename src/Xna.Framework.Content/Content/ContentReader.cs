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
        private ContentManager _contentManager;
        private Action<IDisposable> _recordDisposableObject;
        
        private ContentTypeReaderManager _typeReaderManager;
        private string _assetName;
        private List<KeyValuePair<int, Action<object>>> _sharedResourceFixups;
        private ContentTypeReader[] _typeReaders;
        internal int _version;

        internal ContentTypeReader[] TypeReaders
        {
            get { return _typeReaders; }
        }

        internal ContentReader(ContentManager manager, Stream stream, string assetName, int version, Action<IDisposable> recordDisposableObject)
            : base(stream)
        {
            this._recordDisposableObject = recordDisposableObject;
            this._contentManager = manager;
            this._assetName = assetName;
            this._version = version;
        }

        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }
        
        public string AssetName
        {
            get { return _assetName; }
        }

        public ContentBufferPool BufferPool
        {
            get { return ContentBufferPool.Current; }
        }

        internal T ReadAsset<T>()
        {
            int typeReaderCount = base.Read7BitEncodedInt();
            _typeReaderManager = new ContentTypeReaderManager();
            _typeReaders = _typeReaderManager.LoadAssetReaders(this, typeReaderCount);

            int sharedResourceCount = base.Read7BitEncodedInt();
            _sharedResourceFixups = new List<KeyValuePair<int, Action<object>>>();

            // Read primary object
            T result = ReadObject<T>();

            // Read shared resources
            if (sharedResourceCount > 0)
                ReadSharedResources(sharedResourceCount);
            
            return result;
        }

        internal void ReadSharedResources(int sharedResourceCount)
        {
            object[] sharedResources = new object[sharedResourceCount];

            for (int i = 0; i < sharedResourceCount; ++i)
            {
                object existingInstance = null;
                sharedResources[i] = ReadObject<object>(existingInstance);
            }

            // Fixup shared resources by calling each registered action
            for (int i = 0; i < _sharedResourceFixups.Count; ++i)
            {
                KeyValuePair<int, Action<object>> fixup = _sharedResourceFixups[i];
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
                return _contentManager.Load<T>(FileHelpers.ResolveRelativePath(_assetName, externalReference));
            }

            return default(T);
        }

            
        private void ExecuteRecordDisposable<T>(T result)
        {
            IDisposable disposable = result as IDisposable;
            if (disposable != null)
            {
                if (_recordDisposableObject != null)
                    _recordDisposableObject(disposable);
                else
                    _contentManager.RecordDisposableCallback(disposable);
            }
        }

        public T ReadObject<T>()
        {
            return ReadObject<T>(default(T));
        }

        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            T result;

            ContentTypeReader<T> typeReaderT = typeReader as ContentTypeReader<T>;
            if (typeReaderT != null)
                result = typeReaderT.Read(this, default(T));
            else
                result = (T)typeReader.Read(this, default(T));

            ExecuteRecordDisposable<T>(result);
            return result;
        }

        public T ReadObject<T>(T existingInstance)
        {
            int typeReaderIndex = base.Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;

            if (typeReaderIndex > _typeReaders.Length)
                throw new ContentLoadException("Incorrect type reader index found!");

            ContentTypeReader typeReader = _typeReaders[typeReaderIndex - 1];
            T result = (T)typeReader.Read(this, existingInstance);

            ExecuteRecordDisposable(result);

            return result;
        }

        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            if (!ReflectionHelpers.IsValueType(typeReader.TargetType))
                return ReadObject(existingInstance);

            T result = (T)typeReader.Read(this, existingInstance);

            ExecuteRecordDisposable(result);

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
            foreach(ContentTypeReader typeReader in _typeReaders)
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
            int index = base.Read7BitEncodedInt();
            if (index > 0)
            {
                _sharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(index - 1, delegate(object v)
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
