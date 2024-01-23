// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Content
{
    internal class Vector2Reader : ContentTypeReader<Vector2>
    {
        protected internal override Vector2 Read(ContentReader input, Vector2 existingInstance)
        {
            return input.ReadVector2();
        }
    }
}
