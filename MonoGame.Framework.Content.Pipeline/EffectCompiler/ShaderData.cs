
namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal partial class ShaderData
	{
		public ShaderData(bool isVertexShader, int sharedIndex, byte[] bytecode)
		{
			IsVertexShader = isVertexShader;
			SharedIndex = sharedIndex;
			Bytecode = (byte[])bytecode.Clone();	    
		}

		public bool IsVertexShader { get; private set; }

		public struct Sampler
		{
			public MojoShader.MOJOSHADER_samplerType type;
			public int textureSlot;
            public int samplerSlot;
			public string samplerName;
			public string parameterName;
			public int parameter;
			public SamplerStateContent state;
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

		public byte[] Bytecode { get; private set; }

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
    public enum EffectParameterTypeContent
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

