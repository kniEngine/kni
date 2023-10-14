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
        public string samplerName;
        public string parameterName;
        public int parameter;
        public SamplerState state;

        public override string ToString()
        {
            return String.Format("type: {0}, parameter:{1}, parameterName: {2}, samplerName: {3}, t#: {4}, s#: {5}",
                                 type, parameter, parameterName, samplerName, textureSlot, samplerSlot);
        }
    }
}
