// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Framework.Content
{
    internal class ColorReader : ContentTypeReader<Color>
    {
        protected override Color Read(ContentReader input, Color existingInstance)
        {
            Color result = new Color();
            result.R = input.ReadByte();
            result.G = input.ReadByte();
            result.B = input.ReadByte();
            result.A = input.ReadByte();
            return result;
        }
    }
}
