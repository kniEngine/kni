// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal struct SamplerInfo
    {
        public MojoShader.SamplerType type;
        public int textureSlot;
        public int samplerSlot;
        public string GLsamplerName; // e.g. "ps_s0", "ps_s1", etc
        public string textureName;
        public int textureParameter;
        public SamplerState state;

        public override string ToString()
        {
            return String.Format("type: {0}, textureParameter:{1}, textureName: {2}, GLsamplerName: {3}, t#: {4}, s#: {5}",
                                 type, textureParameter, textureName, GLsamplerName, textureSlot, samplerSlot);
        }
    }
}
