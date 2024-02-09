// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class EffectReader : ContentTypeReader<Effect>
    {
        protected override Effect Read(ContentReader input, Effect existingInstance)
        {
            int dataSize = input.ReadInt32();
            byte[] data = input.BufferPool.Get(dataSize);
            input.Read(data, 0, dataSize);
            Effect effect = existingInstance ?? new Effect(input.GetGraphicsDevice(), data, 0, dataSize);
            input.BufferPool.Return(data);

            return effect;
        }
    }
}
