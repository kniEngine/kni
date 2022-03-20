// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{

    internal class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
    {
        ContentTypeReader keyReader;
		ContentTypeReader valueReader;
		
		Type keyType;
		Type valueType;
		
        public DictionaryReader()
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
			keyType = typeof(TKey);
			valueType = typeof(TValue);
			
			keyReader = manager.GetTypeReader(keyType);
			valueReader = manager.GetTypeReader(valueType);
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

            if (ReflectionHelpers.IsValueType(keyType))
            {
                for (int i = 0; i < count; i++)
                {
                    TKey key = input.ReadObject<TKey>(keyReader);
                    TValue value = input.ReadObject<TValue>(valueReader);
                    dictionary.Add(key, value);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var keyReaderType = input.Read7BitEncodedInt();
                    TKey key = keyReaderType > 0 ? input.ReadObject<TKey>(input.TypeReaders[keyReaderType - 1]) : default(TKey);

                    var valueReaderType = input.Read7BitEncodedInt();
                    TValue value = valueReaderType > 0 ? input.ReadObject<TValue>(input.TypeReaders[valueReaderType - 1]) : default(TValue);

                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
    }
}

