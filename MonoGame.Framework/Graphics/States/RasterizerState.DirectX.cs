// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        private D3D11.RasterizerState _state;

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
                D3D11.RasterizerStateDescription desc = new D3D11.RasterizerStateDescription();

                switch (CullMode)
                {
                    case Graphics.CullMode.None:
                        desc.CullMode = D3D11.CullMode.None;
                        break;

                    case Graphics.CullMode.CullClockwiseFace:
                        desc.CullMode = D3D11.CullMode.Front;
                        break;

                    case Graphics.CullMode.CullCounterClockwiseFace:
                        desc.CullMode = D3D11.CullMode.Back;
                        break;

                    default:
                        throw new InvalidOperationException("CullMode");
                }

                desc.IsScissorEnabled = ScissorTestEnable;
                desc.IsMultisampleEnabled = MultiSampleAntiAlias;

                // discussion and explanation in https://github.com/MonoGame/MonoGame/issues/4826
                DepthFormat activeDepthFormat = (context.IsRenderTargetBound)
                                              ? context._currentRenderTargetBindings[0].DepthFormat
                                              : this.GraphicsDevice.PresentationParameters.DepthStencilFormat;
                int depthMul;
                switch (activeDepthFormat)
                {
                    case DepthFormat.None:
                        depthMul = 0;
                        break;
                    case DepthFormat.Depth16:
                        depthMul = 1 << 16 - 1;
                        break;
                    case DepthFormat.Depth24:
                    case DepthFormat.Depth24Stencil8:
                        depthMul = 1 << 24 - 1;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                desc.DepthBias = (int) (DepthBias * depthMul);
                desc.SlopeScaledDepthBias = SlopeScaleDepthBias;

                if (FillMode == Graphics.FillMode.WireFrame)
                    desc.FillMode = D3D11.FillMode.Wireframe;
                else
                    desc.FillMode = D3D11.FillMode.Solid;

                desc.IsDepthClipEnabled = DepthClipEnable;

                // These are new DX11 features we should consider exposing
                // as part of the extended MonoGame API.
                desc.IsFrontCounterClockwise = false;
                desc.IsAntialiasedLineEnabled = false;

                // To support feature level 9.1 these must 
                // be set to these exact values.
                desc.DepthBiasClamp = 0.0f;

                // Create the state.
                _state = new D3D11.RasterizerState(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, desc);
            }

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state.
            context.D3dContext.Rasterizer.State = _state;
        }

        partial void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

            SharpDX.Utilities.Dispose(ref _state);
        }
    }
}
