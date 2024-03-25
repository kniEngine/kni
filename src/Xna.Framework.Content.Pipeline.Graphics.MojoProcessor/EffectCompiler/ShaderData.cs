// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal class ShaderData
	{
        private readonly ShaderStage _shaderStage;

		public ShaderStage Stage { get { return _shaderStage; } }


        public ShaderData(ShaderStage shaderStage, int sharedIndex)
		{
            _shaderStage = shaderStage;
			SharedIndex = sharedIndex;
		}

		public struct Attribute
		{
            public string name;
            public VertexElementUsage usage;
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

		public SamplerInfo[] _samplers;

		public ShaderData.Attribute[] _attributes;

		public byte[] ShaderCode { get; set; }


#region Non-Serialized Stuff

		// The index of the shader in the shared list.
		public int SharedIndex { get; private set; }

        public string ShaderFunctionName { get; set; }

        public string ShaderProfile { get; set; }

#endregion // Non-Serialized Stuff

	}

}

