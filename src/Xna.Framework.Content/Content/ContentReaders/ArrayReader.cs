// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class ArrayReader<T> : ContentTypeReader<T[]>
    {
        ContentTypeReader _elementReader;

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            Type readerType = typeof(T);
            _elementReader = manager.GetTypeReader(readerType);
        }

        protected internal override T[] Read(ContentReader input, T[] existingInstance)
        {
            uint count = input.ReadUInt32();
            T[] array = existingInstance;
            if (array == null)
                array = new T[count];

            if (ReflectionHelpers.IsValueType(typeof(T)))
            {
                for (uint i = 0; i < count; i++)
                {
                    array[i] = input.ReadObject<T>(_elementReader);
                }
            }
            else
            {
                for (uint i = 0; i < count; i++)
                {
                    int readerType = input.Read7BitEncodedInt();
                    array[i] = readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T);
                }
            }
            return array;
        }
    }
}
