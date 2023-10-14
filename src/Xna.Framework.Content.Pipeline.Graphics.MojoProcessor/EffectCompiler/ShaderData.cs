// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal partial class ShaderData
	{
        private readonly ShaderStage _shaderStage;

		public ShaderStage Stage { get { return _shaderStage; } }


        public ShaderData(ShaderStage shaderStage, int sharedIndex)
		{
            _shaderStage = shaderStage;
			SharedIndex = sharedIndex;
		}


		public struct Sampler
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

		public struct Attribute
		{
            public string name;
            public VertexElementUsageContent usage;
			public int index;
#pragma warning disable 649
            public int location;
#pragma warning restore 649
        }

		/// <summary>
		/// The index to the constant buffers which are 
		/// required by this shader at runtime.
		/// </summary>
		public int[] _cbuffers;

		public Sampler[] _samplers;

		public Attribute[] _attributes;

		public byte[] ShaderCode { get; set; }


#region Non-Serialized Stuff

		// The index of the shader in the shared list.
		public int SharedIndex { get; private set; }

        public string ShaderFunctionName { get; set; }

        public string ShaderProfile { get; set; }

#endregion // Non-Serialized Stuff

	}

    /// <summary>
    /// Defines usage for vertex elements.
    /// </summary>
    internal enum VertexElementUsageContent
    {
        /// <summary>
        /// Position data.
        /// </summary>
        Position,
        /// <summary>
        /// Color data.
        /// </summary>
        Color,
        /// <summary>
        /// Texture coordinate data or can be used for user-defined data.
        /// </summary>
        TextureCoordinate,
        /// <summary>
        /// Normal data.
        /// </summary>
        Normal,
        /// <summary>
        /// Binormal data.
        /// </summary>
        Binormal,
        /// <summary>
        /// Tangent data.
        /// </summary>
        Tangent,
        /// <summary>
        /// Blending indices data.
        /// </summary>
        BlendIndices,
        /// <summary>
        /// Blending weight data.
        /// </summary>
        BlendWeight,
        /// <summary>
        /// Depth data.
        /// </summary>
        Depth,
        /// <summary>
        /// Fog data.
        /// </summary>
        Fog,
        /// <summary>
        /// Point size data. Usable for drawing point sprites.
        /// </summary>
        PointSize,
        /// <summary>
        /// Sampler data for specifies the displacement value to look up.
        /// </summary>
        Sample,
        /// <summary>
        /// Single, positive float value, specifies a tessellation factor used in the tessellation unit to control the rate of tessellation.
        /// </summary>
        TessellateFactor
    }


    /// <summary>
    /// Defines types for effect parameters and shader constants.
    /// </summary>
    enum EffectParameterTypeContent
    {
        /// <summary>
        /// Pointer to void type.
        /// </summary>
		Void,
        /// <summary>
        /// Boolean type. Any non-zero will be <c>true</c>; <c>false</c> otherwise.
        /// </summary>
		Bool,
        /// <summary>
        /// 32-bit integer type.
        /// </summary>
		Int32,
        /// <summary>
        /// Float type.
        /// </summary>
		Single,
        /// <summary>
        /// String type.
        /// </summary>
		String,
        /// <summary>
        /// Any texture type.
        /// </summary>
		Texture,
        /// <summary>
        /// 1D-texture type.
        /// </summary>
        Texture1D,
        /// <summary>
        /// 2D-texture type.
        /// </summary>
        Texture2D,
        /// <summary>
        /// 3D-texture type.
        /// </summary>
        Texture3D,
        /// <summary>
        /// Cubic texture type.
        /// </summary>
		TextureCube
    }
}

