// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Framework.Content
{
    class BoundingBoxReader : ContentTypeReader<BoundingBox>
    {
        protected internal override BoundingBox Read(ContentReader input, BoundingBox existingInstance)
        {
            Vector3 min = input.ReadVector3();
            Vector3 max = input.ReadVector3();

            BoundingBox result = new BoundingBox(min, max);
            return result;
        }
    }
}
