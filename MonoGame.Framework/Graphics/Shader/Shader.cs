// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{

    // TODO: We should convert the types below 
    // into the start of a Shader reflection API.

    internal enum SamplerType
    {
        Sampler2D = 0,
        SamplerCube = 1,
        SamplerVolume = 2,
        Sampler1D = 3,
    }

    internal struct SamplerInfo
    {
        public SamplerType type;
        public int textureSlot;
        public int samplerSlot;
        public string name;
		public SamplerState state;

        // TODO: This should be moved to EffectPass.
        public int parameter;
    }

    internal struct VertexAttribute
    {
        public VertexElementUsage usage;
        public int index;
        public string name;
        public int location;
    }

    internal partial class Shader : GraphicsResource
	{
        private ShaderStrategy _strategy;

        internal ShaderStrategy Strategy { get { return _strategy; } }
        
        /// <summary>
        /// A hash value which can be used to compare shaders.
        /// </summary>
        internal int HashKey { get; private set; }

        public SamplerInfo[] Samplers { get; private set; }

	    public int[] CBuffers { get; private set; }

        public ShaderStage Stage { get; private set; }

        public VertexAttribute[] Attributes { get; private set; }

        internal Shader(GraphicsDevice graphicsDevice,
            ShaderStage stage, byte[] shaderBytecode,
            SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes,
            ShaderProfileType profile)
            : base(true)
        {
            _strategy = graphicsDevice.CurrentContext.Strategy.CreateShaderStrategy(stage, shaderBytecode, samplers, cBuffers, attributes, profile);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
            
            PlatformValidateProfile(profile);

            this.Stage = stage;
            this.Samplers = samplers;
            this.CBuffers = cBuffers;
            this.Attributes = attributes;

            PlatformConstructShader(stage, shaderBytecode);
        }

        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }
	}
}

