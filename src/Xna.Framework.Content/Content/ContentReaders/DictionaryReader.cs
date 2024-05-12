// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Framework.Content
{

    internal class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
    {
        ContentTypeReader _keyReader;
        ContentTypeReader _valueReader;
        
        Type _keyType;
        Type _valueType;
        
        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            _keyType = typeof(TKey);
            _valueType = typeof(TValue);
            
            _keyReader = manager.GetTypeReader(_keyType);
            _valueReader = manager.GetTypeReader(_valueType);
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override Dictionary<TKey, TValue> Read(ContentReader input, Dictionary<TKey, TValue> existingInstance)
        {
            int count = input.ReadInt32();

            Dictionary<TKey, TValue> dictionary = existingInstance;
            if (dictionary == null)
                dictionary = new Dictionary<TKey, TValue>(count);
            else
                dictionary.Clear();

            if (ReflectionHelpers.IsValueType(_keyType))
            {
                for (int i = 0; i < count; i++)
                {
                    TKey key = input.ReadObject<TKey>(_keyReader);

                    TValue value;
                    if (ReflectionHelpers.IsValueType(_valueType))
                    {
                        value = input.ReadObject<TValue>(_valueReader);
                    }
                    else
                    {
                        int valueReaderType = input.Read7BitEncodedInt();
                        value = (valueReaderType) > 0
                              ? input.ReadObject<TValue>(input.TypeReaders[valueReaderType-1])
                              : default(TValue);
                    }

                    dictionary.Add(key, value);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int keyReaderType = input.Read7BitEncodedInt();
                    TKey key = (keyReaderType > 0)
                             ? input.ReadObject<TKey>(input.TypeReaders[keyReaderType - 1])
                             : default(TKey);

                    TValue value;
                    if (ReflectionHelpers.IsValueType(_valueType))
                    {
                        value = input.ReadObject<TValue>(_valueReader);
                    }
                    else
                    {
                        int valueReaderType = input.Read7BitEncodedInt();
                        value = (valueReaderType > 0)
                              ? input.ReadObject<TValue>(input.TypeReaders[valueReaderType-1])
                              : default(TValue);
                    }

                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
    }
}

