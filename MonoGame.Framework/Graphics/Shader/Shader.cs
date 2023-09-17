// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{

    internal class Shader : GraphicsResource
	{
        private ShaderStrategy _strategy;

        internal ShaderStrategy Strategy { get { return _strategy; } }
        
        /// <summary>
        /// A hash value which can be used to compare shaders.
        /// </summary>
        internal int HashKey { get { return _strategy.HashKey; } }

        public SamplerInfo[] Samplers { get { return _strategy.Samplers; } }

        public int[] CBuffers { get { return _strategy.CBuffers; } }

        public ShaderStage Stage { get { return _strategy.Stage; } }

        public VertexAttribute[] Attributes { get { return _strategy.Attributes; } }


        internal Shader(GraphicsDevice graphicsDevice,
            ShaderStage stage, byte[] shaderBytecode,
            SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes,
            ShaderProfileType profile)
            : base(true)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    _strategy = graphicsDevice.CurrentContext.Strategy.CreateVertexShaderStrategy(shaderBytecode, samplers, cBuffers, attributes, profile);
                    break;
                case ShaderStage.Pixel:
                    _strategy = graphicsDevice.CurrentContext.Strategy.CreatePixelShaderStrategy(shaderBytecode, samplers, cBuffers, attributes, profile);
                    break;

                default:
                    throw new InvalidOperationException("stage");
            }

            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

        internal protected override void GraphicsDeviceResetting()
        {
        }
	}
}

