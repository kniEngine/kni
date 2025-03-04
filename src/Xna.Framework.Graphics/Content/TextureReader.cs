// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class TextureReader : ContentTypeReader<Texture>
    {
        protected override Texture Read(ContentReader input, Texture existingInstance)
        {
            return existingInstance;
        }
    }
}