// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Content
{
    internal class Int64Reader : ContentTypeReader<long>
    {
        protected internal override long Read(ContentReader input, long existingInstance)
        {
            return input.ReadInt64();
        }
    }
}
