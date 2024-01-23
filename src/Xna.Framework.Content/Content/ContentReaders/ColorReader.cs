// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Framework.Content
{
    internal class ColorReader : ContentTypeReader<Color>
    {
        protected internal override Color Read(ContentReader input, Color existingInstance)
        {
            return input.ReadColor();
        }
    }
}
