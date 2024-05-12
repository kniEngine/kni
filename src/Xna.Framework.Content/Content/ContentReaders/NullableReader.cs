// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class NullableReader<T> : ContentTypeReader<T?> where T : struct
    {
        ContentTypeReader _elementReader;

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {			
            Type readerType = typeof(T);
            _elementReader = manager.GetTypeReader(readerType);
        }
        
        protected internal override T? Read(ContentReader input, T? existingInstance)
        {
            if(input.ReadBoolean())
                return input.ReadObject<T>(_elementReader);
            
            return null;
        }
    }
}

