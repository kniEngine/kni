// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos


namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public class SamplerStateContent //: ContentItem
    {
        public TextureAddressModeContent AddressU;
        public TextureAddressModeContent AddressV;
        public TextureAddressModeContent AddressW;
        public Color BorderColor;
        public int MaxAnisotropy;
        public int MaxMipLevel;
        public float MipMapLevelOfDetailBias;
        public TextureFilterContent Filter;
        public TextureFilterModeContent FilterMode;
        public CompareFunctionContent ComparisonFunction;

        public SamplerStateContent()
        {
            AddressU = TextureAddressModeContent.Wrap;
            AddressV = TextureAddressModeContent.Wrap;
            AddressW = TextureAddressModeContent.Wrap;
            BorderColor = Color.White;
            MaxAnisotropy = 4;
            MaxMipLevel = 0;
            MipMapLevelOfDetailBias = 0.0f;
            Filter = TextureFilterContent.Linear;
            FilterMode = TextureFilterModeContent.Default;
            ComparisonFunction = CompareFunctionContent.Never;
        }

    }

    /// <summary>
    /// Defines modes for addressing texels using texture coordinates that are outside of the range of 0.0 to 1.0.
    /// </summary>
    public enum TextureAddressModeContent
    {
        /// <summary>
        /// Texels outside range will form the tile at every integer junction.
        /// </summary>
        Wrap,
        /// <summary>
        /// Texels outside range will be set to color of 0.0 or 1.0 texel.
        /// </summary>
        Clamp,
        /// <summary>
        /// Same as <see cref="TextureAddressModeContent.Wrap"/> but tiles will also flipped at every integer junction.
        /// </summary>
        Mirror,
        /// <summary>
        /// Texels outside range will be set to the border color.
        /// </summary>
        Border
    }
    
    public enum TextureFilterTypeContent
    {
        None,
        Point,
        Linear,
        Anisotropic,
    }

    /// <summary>
    /// Defines filtering types for texture sampler.
    /// </summary>
    public enum TextureFilterContent
    {
        /// <summary>
        /// Use linear filtering.
        /// </summary>
		Linear,
        /// <summary>
        /// Use point filtering.
        /// </summary>
		Point,
        /// <summary>
        /// Use anisotropic filtering.
        /// </summary>
		Anisotropic,
        /// <summary>
        /// Use linear filtering to shrink or expand, and point filtering between mipmap levels (mip).
        /// </summary>
        LinearMipPoint,
        /// <summary>
        /// Use point filtering to shrink (minify) or expand (magnify), and linear filtering between mipmap levels.
        /// </summary>
		PointMipLinear,
        /// <summary>
        /// Use linear filtering to shrink, point filtering to expand, and linear filtering between mipmap levels.
        /// </summary>
		MinLinearMagPointMipLinear,
        /// <summary>
        /// Use linear filtering to shrink, point filtering to expand, and point filtering between mipmap levels.
        /// </summary>
		MinLinearMagPointMipPoint,
        /// <summary>
        /// Use point filtering to shrink, linear filtering to expand, and linear filtering between mipmap levels.
        /// </summary>
		MinPointMagLinearMipLinear,
        /// <summary>
        /// Use point filtering to shrink, linear filtering to expand, and point filtering between mipmap levels.
        /// </summary>
		MinPointMagLinearMipPoint
    }

    /// <summary>
    /// Filtering modes for texture samplers.
    /// </summary>
    public enum TextureFilterModeContent
    {
        Default,
        Comparison
    }

    /// <summary>
    /// The comparison function used for depth, stencil, and alpha tests.
    /// </summary>
    public enum CompareFunctionContent
    {
        /// <summary>
        /// Always passes the test.
        /// </summary>
        Always,
        /// <summary>
        /// Never passes the test.
        /// </summary>
        Never,
        /// <summary>
        /// Passes the test when the new pixel value is less than current pixel value.
        /// </summary>
        Less,
        /// <summary>
        /// Passes the test when the new pixel value is less than or equal to current pixel value.
        /// </summary>
        LessEqual,
        /// <summary>
        /// Passes the test when the new pixel value is equal to current pixel value.
        /// </summary>
        Equal,
        /// <summary>
        /// Passes the test when the new pixel value is greater than or equal to current pixel value.
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// Passes the test when the new pixel value is greater than current pixel value.
        /// </summary>
        Greater,
        /// <summary>
        /// Passes the test when the new pixel value does not equal to current pixel value.
        /// </summary>
        NotEqual
    }
}
