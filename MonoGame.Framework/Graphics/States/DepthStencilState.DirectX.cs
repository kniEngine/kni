// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        private SharpDX.Direct3D11.DepthStencilState _state;

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal void PlatformApplyState(ConcreteGraphicsContext context)
        {
            if (_state == null)
            {
                // Build the description.
                var desc = new SharpDX.Direct3D11.DepthStencilStateDescription();

                desc.IsDepthEnabled = DepthBufferEnable;
                desc.DepthComparison = DepthBufferFunction.ToDXComparisonFunction();

                if (DepthBufferWriteEnable)
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All;
                else
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.Zero;

                desc.IsStencilEnabled = StencilEnable;
                desc.StencilReadMask = (byte)StencilMask; // TODO: Should this instead grab the upper 8bits?
                desc.StencilWriteMask = (byte)StencilWriteMask;

                if (TwoSidedStencilMode)
                {
                    desc.BackFace.Comparison = CounterClockwiseStencilFunction.ToDXComparisonFunction();
                    desc.BackFace.DepthFailOperation = ToDXStencilOp(CounterClockwiseStencilDepthBufferFail);
                    desc.BackFace.FailOperation = ToDXStencilOp(CounterClockwiseStencilFail);
                    desc.BackFace.PassOperation = ToDXStencilOp(CounterClockwiseStencilPass);
                }
                else
                {   //use same settings as frontFace 
                    desc.BackFace.Comparison = StencilFunction.ToDXComparisonFunction();
                    desc.BackFace.DepthFailOperation = ToDXStencilOp(StencilDepthBufferFail);
                    desc.BackFace.FailOperation = ToDXStencilOp(StencilFail);
                    desc.BackFace.PassOperation = ToDXStencilOp(StencilPass);
                }

                desc.FrontFace.Comparison = StencilFunction.ToDXComparisonFunction();
                desc.FrontFace.DepthFailOperation = ToDXStencilOp(StencilDepthBufferFail);
                desc.FrontFace.FailOperation = ToDXStencilOp(StencilFail);
                desc.FrontFace.PassOperation = ToDXStencilOp(StencilPass);

                // Create the state.
                _state = new SharpDX.Direct3D11.DepthStencilState(GraphicsDevice.D3DDevice, desc);
            }

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state.
            context.D3dContext.OutputMerger.SetDepthStencilState(_state, ReferenceStencil);
        }

        static private SharpDX.Direct3D11.StencilOperation ToDXStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Decrement:
                    return SharpDX.Direct3D11.StencilOperation.Decrement;
                case StencilOperation.DecrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.DecrementAndClamp;
                case StencilOperation.Increment:
                    return SharpDX.Direct3D11.StencilOperation.Increment;
                case StencilOperation.IncrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.IncrementAndClamp;
                case StencilOperation.Invert:
                    return SharpDX.Direct3D11.StencilOperation.Invert;
                case StencilOperation.Keep:
                    return SharpDX.Direct3D11.StencilOperation.Keep;
                case StencilOperation.Replace:
                    return SharpDX.Direct3D11.StencilOperation.Replace;
                case StencilOperation.Zero:
                    return SharpDX.Direct3D11.StencilOperation.Zero;

                default:
                    throw new ArgumentException("Invalid stencil operation!");
            }
        }

        partial void PlatformDispose()
        {
            SharpDX.Utilities.Dispose(ref _state);
        }
    }
}

