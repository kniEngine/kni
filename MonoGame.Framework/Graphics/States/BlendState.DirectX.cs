// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        private D3D11.BlendState _state;

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal D3D11.BlendState GetDxState(ConcreteGraphicsContext context)
        {
            if (_state == null)
            {
                // Build the description.
                D3D11.BlendStateDescription desc = new D3D11.BlendStateDescription();
                _targetBlendState[0].GetState(ref desc.RenderTarget[0]);
                _targetBlendState[1].GetState(ref desc.RenderTarget[1]);
                _targetBlendState[2].GetState(ref desc.RenderTarget[2]);
                _targetBlendState[3].GetState(ref desc.RenderTarget[3]);
                desc.IndependentBlendEnable = _independentBlendEnable;

                // This is a new DX11 feature we should consider 
                // exposing as part of the extended MonoGame API.
                desc.AlphaToCoverageEnable = false;

                // Create the state.
                _state = new D3D11.BlendState(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, desc);
            }

			// Apply the state!
			return _state;
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

