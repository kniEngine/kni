// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public enum SamplerType
    {
        Sampler2D = 0,
        SamplerCube = 1,
        SamplerVolume = 2,
        Sampler1D = 3,
    }

    public struct SamplerInfo
    {
        public SamplerType type;
        public int textureSlot;
        public int samplerSlot;
        public string GLsamplerName; // e.g. "ps_s0", "ps_s1", etc
        public SamplerState state;

        // TODO: This should be moved to EffectPass.
        public int textureParameter;

        public override string ToString()
        {
            return String.Format("type: {0}, textureParameter:{1}, GLsamplerName: {2}, t#: {3}, s#: {4}",
                                 type, textureParameter, GLsamplerName, textureSlot, samplerSlot);
        }
    }
}

