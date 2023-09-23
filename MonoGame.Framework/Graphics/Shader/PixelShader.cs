// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class PixelShader : Shader
    {
        internal PixelShader(GraphicsDevice graphicsDevice,
            byte[] shaderBytecode,
            SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes,
            ShaderProfileType profile)
            : base()
        {
            _strategy = graphicsDevice.CurrentContext.Strategy.CreatePixelShaderStrategy(shaderBytecode, samplers, cBuffers, attributes, profile);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

	}
}

