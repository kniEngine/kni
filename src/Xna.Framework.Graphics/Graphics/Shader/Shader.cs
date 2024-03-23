// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{

    public abstract class Shader : GraphicsResource
        , IPlatformShader
    {
        protected ShaderStrategy _strategy;

        ShaderStrategy IPlatformShader.Strategy { get { return _strategy; } }
        
        public SamplerInfo[] Samplers { get { return _strategy.Samplers; } }

        public int[] CBuffers { get { return _strategy.CBuffers; } }

        public VertexAttribute[] Attributes { get { return _strategy.Attributes; } }


        internal Shader()
            : base()
        {
        }

    }
}

