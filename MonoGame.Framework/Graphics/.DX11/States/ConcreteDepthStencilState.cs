// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteDepthStencilState : ResourceDepthStencilStateStrategy
    {
        private D3D11.DepthStencilState _state;

        internal ConcreteDepthStencilState(GraphicsContextStrategy contextStrategy, IDepthStencilStateStrategy source)
            : base(contextStrategy, source)
        {
            _state = CreateDXState(base.GraphicsDeviceStrategy);
        }

        internal D3D11.DepthStencilState GetDxState()
        {
            return _state;
        }

        internal D3D11.DepthStencilState CreateDXState(GraphicsDeviceStrategy deviceStrategy)
        {
            // Build the description.
            D3D11.DepthStencilStateDescription depthStencilStateDesc = new D3D11.DepthStencilStateDescription();

            depthStencilStateDesc.IsDepthEnabled = DepthBufferEnable;
            depthStencilStateDesc.DepthComparison = DepthBufferFunction.ToDXComparisonFunction();

            if (DepthBufferWriteEnable)
                depthStencilStateDesc.DepthWriteMask = D3D11.DepthWriteMask.All;
            else
                depthStencilStateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;

            depthStencilStateDesc.IsStencilEnabled = StencilEnable;
            depthStencilStateDesc.StencilReadMask = (byte)StencilMask; // TODO: Should this instead grab the upper 8bits?
            depthStencilStateDesc.StencilWriteMask = (byte)StencilWriteMask;

            if (TwoSidedStencilMode)
            {
                depthStencilStateDesc.BackFace.Comparison = CounterClockwiseStencilFunction.ToDXComparisonFunction();
                depthStencilStateDesc.BackFace.DepthFailOperation = ToDXStencilOp(CounterClockwiseStencilDepthBufferFail);
                depthStencilStateDesc.BackFace.FailOperation = ToDXStencilOp(CounterClockwiseStencilFail);
                depthStencilStateDesc.BackFace.PassOperation = ToDXStencilOp(CounterClockwiseStencilPass);
            }
            else
            {   //use same settings as frontFace 
                depthStencilStateDesc.BackFace.Comparison = StencilFunction.ToDXComparisonFunction();
                depthStencilStateDesc.BackFace.DepthFailOperation = ToDXStencilOp(StencilDepthBufferFail);
                depthStencilStateDesc.BackFace.FailOperation = ToDXStencilOp(StencilFail);
                depthStencilStateDesc.BackFace.PassOperation = ToDXStencilOp(StencilPass);
            }

            depthStencilStateDesc.FrontFace.Comparison = StencilFunction.ToDXComparisonFunction();
            depthStencilStateDesc.FrontFace.DepthFailOperation = ToDXStencilOp(StencilDepthBufferFail);
            depthStencilStateDesc.FrontFace.FailOperation = ToDXStencilOp(StencilFail);
            depthStencilStateDesc.FrontFace.PassOperation = ToDXStencilOp(StencilPass);

            // Create the state.
            return new D3D11.DepthStencilState(deviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilStateDesc);
        }

        static private D3D11.StencilOperation ToDXStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Decrement:
                    return D3D11.StencilOperation.Decrement;
                case StencilOperation.DecrementSaturation:
                    return D3D11.StencilOperation.DecrementAndClamp;
                case StencilOperation.Increment:
                    return D3D11.StencilOperation.Increment;
                case StencilOperation.IncrementSaturation:
                    return D3D11.StencilOperation.IncrementAndClamp;
                case StencilOperation.Invert:
                    return D3D11.StencilOperation.Invert;
                case StencilOperation.Keep:
                    return D3D11.StencilOperation.Keep;
                case StencilOperation.Replace:
                    return D3D11.StencilOperation.Replace;
                case StencilOperation.Zero:
                    return D3D11.StencilOperation.Zero;

                default:
                    throw new ArgumentException("Invalid stencil operation!");
            }
        }


        protected override void PlatformGraphicsContextLost()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            DX.Utilities.Dispose(ref _state);

            base.Dispose(disposing);
        }
    }

}
