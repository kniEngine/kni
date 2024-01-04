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
    internal class ConcreteRasterizerState : ResourceRasterizerStateStrategy
    {
        private D3D11.RasterizerState _state;

        internal ConcreteRasterizerState(GraphicsContextStrategy contextStrategy, IRasterizerStateStrategy source)
            : base(contextStrategy, source)
        {
            // discussion and explanation in https://github.com/MonoGame/MonoGame/issues/4826
            // TODO: Need fixing. We create the rasterizerStateDesc based on
            // the DepthFormat of the current rendertarget. When the user set
            // a new rendertarget the DepthBias will be wrong.
            DepthFormat activeDepthFormat = (contextStrategy.IsRenderTargetBound)
                                          ? contextStrategy._currentRenderTargetBindings[0].DepthFormat
                                          : this.GraphicsDevice.Strategy.PresentationParameters.DepthStencilFormat;

            _state = CreateDXState(this.GraphicsDevice.Strategy, activeDepthFormat);
        }

        internal D3D11.RasterizerState GetDxState(ConcreteGraphicsContext context)
        {
            DepthFormat activeDepthFormat = (context.IsRenderTargetBound)
                                          ? context._currentRenderTargetBindings[0].DepthFormat
                                          : this.GraphicsDevice.Strategy.PresentationParameters.DepthStencilFormat;

            return _state;
        }

        internal D3D11.RasterizerState CreateDXState(GraphicsDeviceStrategy deviceStrategy, DepthFormat activeDepthFormat)
        {
            // Build the description.
            D3D11.RasterizerStateDescription rasterizerStateDesc = new D3D11.RasterizerStateDescription();

            switch (CullMode)
            {
                case CullMode.None:
                    rasterizerStateDesc.CullMode = D3D11.CullMode.None;
                    break;
                case CullMode.CullClockwiseFace:
                    rasterizerStateDesc.CullMode = D3D11.CullMode.Front;
                    break;
                case CullMode.CullCounterClockwiseFace:
                    rasterizerStateDesc.CullMode = D3D11.CullMode.Back;
                    break;

                default:
                    throw new InvalidOperationException("CullMode");
            }

            rasterizerStateDesc.IsScissorEnabled = ScissorTestEnable;
            rasterizerStateDesc.IsMultisampleEnabled = MultiSampleAntiAlias;

            int depthMul;
            switch (activeDepthFormat)
            {
                case DepthFormat.None:
                    depthMul = 0;
                    break;
                case DepthFormat.Depth16:
                    depthMul = (1 << 16) - 1;
                    break;
                case DepthFormat.Depth24:
                case DepthFormat.Depth24Stencil8:
                    depthMul = (1 << 24) - 1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            rasterizerStateDesc.DepthBias = (int)(DepthBias * depthMul);
            rasterizerStateDesc.SlopeScaledDepthBias = SlopeScaleDepthBias;

            if (FillMode == FillMode.WireFrame)
                rasterizerStateDesc.FillMode = D3D11.FillMode.Wireframe;
            else
                rasterizerStateDesc.FillMode = D3D11.FillMode.Solid;

            rasterizerStateDesc.IsDepthClipEnabled = DepthClipEnable;

            // These are new DX11 features we should consider exposing
            // as part of the extended MonoGame API.
            rasterizerStateDesc.IsFrontCounterClockwise = false;
            rasterizerStateDesc.IsAntialiasedLineEnabled = false;

            // To support feature level 9.1 these must 
            // be set to these exact values.
            rasterizerStateDesc.DepthBiasClamp = 0.0f;

            // Create the state.
            return new D3D11.RasterizerState(deviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, rasterizerStateDesc);
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
