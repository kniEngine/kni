// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class ListReader<T> : ContentTypeReader<List<T>>
    {
        ContentTypeReader _elementReader;

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            Type readerType = typeof(T);
            _elementReader = manager.GetTypeReader(readerType);
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override List<T> Read(ContentReader input, List<T> existingInstance)
        {
            int count = input.ReadInt32();
            List<T> list = existingInstance;
            if (list == null) list = new List<T>(count);

            if (ReflectionHelpers.IsValueType(typeof(T)))
            {
                for (int i = 0; i < count; i++)
                {
                    list.Add(input.ReadObject<T>(_elementReader));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int readerType = input.Read7BitEncodedInt();
                    list.Add(readerType > 0 ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T));
                }
            }
            return list;
        }
    }
}
