using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser
{
    public class PassInfo
    {
        public string name;

        public string vsModel;
        public string vsFunction;

        public string psModel;
        public string psFunction;

        public BlendState blendState;
        public RasterizerState rasterizerState;
        public DepthStencilState depthStencilState;
		
        private static Blend ToAlphaBlend(BlendContent blend)
        {
            switch (blend)
            {
                case BlendContent.SourceColor:
                    return Blend.SourceAlpha;
                case BlendContent.InverseSourceColor:
                    return Blend.InverseSourceAlpha;
                case BlendContent.DestinationColor:
                    return Blend.DestinationAlpha;
                case BlendContent.InverseDestinationColor:
                    return Blend.InverseDestinationAlpha;
            }
            return (Blend)blend;
        }

        public bool AlphaBlendEnable
        {
            set
            {
                if (value)
                {
                    if (blendState == null)
                    {
                        blendState = new BlendState();
                        blendState.ColorSourceBlend = Blend.One;
                        blendState.AlphaSourceBlend = Blend.One;
                        blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                    }
                }
                else if (!value)
                {
                    if (blendState == null)
                        blendState = new BlendState();
                    blendState.ColorSourceBlend = Blend.One;
                    blendState.AlphaSourceBlend = Blend.One;
                    blendState.ColorDestinationBlend = Blend.Zero;
                    blendState.AlphaDestinationBlend = Blend.Zero;
                }
            }
        }

        public FillMode FillMode
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.FillMode = value;             
            }
        }

        public CullMode CullMode
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.CullMode = value;
            }
        }

        public bool ZEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferEnable = value;
            }
        }

        public bool ZWriteEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferWriteEnable = value;
            }
        }

        public CompareFunctionContent DepthBufferFunction
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferFunction = (CompareFunction)value;
            }
        }

        public bool MultiSampleAntiAlias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.MultiSampleAntiAlias = value;
            }
        }

        public bool ScissorTestEnable
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.ScissorTestEnable = value;
            }
        }

        public bool StencilEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilEnable = value;
            }
        }

        public StencilOperationContent StencilFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilFail = (StencilOperation)value;
            }
        }

        public CompareFunctionContent StencilFunc
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilFunction = (CompareFunction)value;
            }
        }

        public int StencilMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilMask = value;
            }
        }

        public StencilOperationContent StencilPass
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilPass = (StencilOperation)value;
            }
        }

        public int StencilRef
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.ReferenceStencil = value;
            }
        }

        public int StencilWriteMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilWriteMask = value;
            }
        }

        public StencilOperationContent StencilZFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilDepthBufferFail = (StencilOperation)value;
            }
        }

        public BlendContent SrcBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorSourceBlend = (Blend)value;
                blendState.AlphaSourceBlend = ToAlphaBlend(value);
            }
        }

        public BlendContent DestBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorDestinationBlend = (Blend)value;
                blendState.AlphaDestinationBlend = ToAlphaBlend(value);
            }
        }

        public BlendFunctionContent BlendOp
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.AlphaBlendFunction = (BlendFunction)value;
            }
        }

        public ColorWriteChannels ColorWriteEnable
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorWriteChannels = value;
            }    
        }

        public float DepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.DepthBias = value;
            }
        }

        public float SlopeScaleDepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.SlopeScaleDepthBias = value;
            }
        }
    }


    /// <summary>
    /// Defines stencil buffer operations.
    /// </summary>
    public enum StencilOperationContent
    {
        /// <summary>
        /// Does not update the stencil buffer entry.
        /// </summary>
        Keep,
        /// <summary>
        /// Sets the stencil buffer entry to 0.
        /// </summary>
        Zero,
        /// <summary>
        /// Replaces the stencil buffer entry with a reference value.
        /// </summary>
        Replace,
        /// <summary>
        /// Increments the stencil buffer entry, wrapping to 0 if the new value exceeds the maximum value.
        /// </summary>
        Increment,
        /// <summary>
        /// Decrements the stencil buffer entry, wrapping to the maximum value if the new value is less than 0.
        /// </summary>
        Decrement,
        /// <summary>
        /// Increments the stencil buffer entry, clamping to the maximum value.
        /// </summary>
        IncrementSaturation,
        /// <summary>
        /// Decrements the stencil buffer entry, clamping to 0.
        /// </summary>
        DecrementSaturation,
        /// <summary>
        /// Inverts the bits in the stencil buffer entry.
        /// </summary>
        Invert
    }

    /// <summary>
    /// Defines a blend mode.
    /// </summary>
    public enum BlendContent
    {
        /// <summary>
        /// Each component of the color is multiplied by {1, 1, 1, 1}.
        /// </summary>
        One,
        /// <summary>
        /// Each component of the color is multiplied by {0, 0, 0, 0}.
        /// </summary>
        Zero,
        /// <summary>
        /// Each component of the color is multiplied by the source color. 
        /// {Rs, Gs, Bs, As}, where Rs, Gs, Bs, As are color source values.
        /// </summary>
        SourceColor,
        /// <summary>
        /// Each component of the color is multiplied by the inverse of the source color.
        ///  {1 − Rs, 1 − Gs, 1 − Bs, 1 − As}, where Rs, Gs, Bs, As are color source values.
        /// </summary>
        InverseSourceColor,
        /// <summary>
        /// Each component of the color is multiplied by the alpha value of the source. 
        /// {As, As, As, As}, where As is the source alpha value.
        /// </summary>
        SourceAlpha,
        /// <summary>
        /// Each component of the color is multiplied by the inverse of the alpha value of the source. 
        /// {1 − As, 1 − As, 1 − As, 1 − As}, where As is the source alpha value.
        /// </summary>
        InverseSourceAlpha,
        /// <summary>
        /// Each component color is multiplied by the destination color. 
        /// {Rd, Gd, Bd, Ad}, where Rd, Gd, Bd, Ad are color destination values.
        /// </summary>
        DestinationColor,
        /// <summary>
        /// Each component of the color is multiplied by the inversed destination color. 
        /// {1 − Rd, 1 − Gd, 1 − Bd, 1 − Ad}, where Rd, Gd, Bd, Ad are color destination values.
        /// </summary>
        InverseDestinationColor,
        /// <summary>
        /// Each component of the color is multiplied by the alpha value of the destination.
        /// {Ad, Ad, Ad, Ad}, where Ad is the destination alpha value.
        /// </summary>
        DestinationAlpha,
        /// <summary>
        /// Each component of the color is multiplied by the inversed alpha value of the destination. 
        /// {1 − Ad, 1 − Ad, 1 − Ad, 1 − Ad}, where Ad is the destination alpha value.
        /// </summary>
        InverseDestinationAlpha,
        /// <summary>
        /// Each component of the color is multiplied by a constant in the <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/>.
        /// </summary>
        BlendFactor,
        /// <summary>
        /// Each component of the color is multiplied by a inversed constant in the <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/>.
        /// </summary>
        InverseBlendFactor,
        /// <summary>
        /// Each component of the color is multiplied by either the alpha of the source color, or the inverse of the alpha of the source color, whichever is greater. 
        /// {f, f, f, 1}, where f = min(As, 1 − As), where As is the source alpha value.
        /// </summary>
        SourceAlphaSaturation
    }

    /// <summary>
    /// Defines a function for color blending.
    /// </summary>
    public enum BlendFunctionContent
    {
        /// <summary>
        /// The function will adds destination to the source. (srcColor * srcBlend) + (destColor * destBlend)
        /// </summary>
        Add,
        /// <summary>
        /// The function will subtracts destination from source. (srcColor * srcBlend) − (destColor * destBlend)
        /// </summary>
        Subtract,
        /// <summary>
        /// The function will subtracts source from destination. (destColor * destBlend) - (srcColor * srcBlend) 
        /// </summary>
        ReverseSubtract,
        /// <summary>
        /// The function will extracts minimum of the source and destination. min((srcColor * srcBlend),(destColor * destBlend))
        /// </summary>
        Min,
        /// <summary>
        /// The function will extracts maximum of the source and destination. max((srcColor * srcBlend),(destColor * destBlend))
        /// </summary>
        Max
    }
}