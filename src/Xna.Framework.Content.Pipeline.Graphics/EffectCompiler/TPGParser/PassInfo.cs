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
		
        private static Blend ToAlphaBlend(Blend blend)
        {
            switch (blend)
            {
                case Blend.SourceColor:
                    return Blend.SourceAlpha;
                case Blend.InverseSourceColor:
                    return Blend.InverseSourceAlpha;
                case Blend.DestinationColor:
                    return Blend.DestinationAlpha;
                case Blend.InverseDestinationColor:
                    return Blend.InverseDestinationAlpha;
            }
            return blend;
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

        public Blend SrcBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorSourceBlend = value;
                blendState.AlphaSourceBlend = ToAlphaBlend(value);
            }
        }

        public Blend DestBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorDestinationBlend = value;
                blendState.AlphaDestinationBlend = ToAlphaBlend(value);
            }
        }

        public BlendFunction BlendOp
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.AlphaBlendFunction = value;
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
}