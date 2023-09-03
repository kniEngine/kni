// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D : IRenderTargetDX11
    {
        private int _currentSlice;
        private D3D11.RenderTargetView _renderTargetView;
        private D3D11.DepthStencilView _depthStencilView;

        private void PlatformConstructRenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);
            RenderTargetUsage = usage;

            // Setup the multisampling description.
            DXGI.SampleDescription multisampleDesc = new DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                multisampleDesc.Count = MultiSampleCount;
                multisampleDesc.Quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, new D3D11.Texture2DDescription
            {
                Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                BindFlags = D3D11.BindFlags.DepthStencil,
            }))
            {
                // Create the view for binding to the device.
                _depthStencilView = new D3D11.DepthStencilView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthBuffer, new D3D11.DepthStencilViewDescription()
                {
                    Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat),
                    Dimension = D3D11.DepthStencilViewDimension.Texture2D
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _renderTargetView);
                SharpDX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }

        D3D11.RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
	    {
            if (arraySlice >= Depth)
                throw new ArgumentOutOfRangeException("The arraySlice is out of range for this Texture3D.");

            // Dispose the previous target.
	        if (_currentSlice != arraySlice && _renderTargetView != null)
	        {
	            _renderTargetView.Dispose();
	            _renderTargetView = null;
	        }

            // Create the new target view interface.
	        if (_renderTargetView == null)
	        {
	            _currentSlice = arraySlice;

                D3D11.RenderTargetViewDescription desc = new D3D11.RenderTargetViewDescription
	            {
	                Format = GraphicsExtensions.ToDXFormat(this.Format),
	                Dimension = D3D11.RenderTargetViewDimension.Texture3D,
	                Texture3D =
	                    {
	                        DepthSliceCount = -1,
	                        FirstDepthSlice = arraySlice,
	                        MipSlice = 0,
	                    }
	            };

	            _renderTargetView = new D3D11.RenderTargetView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, GetTexture(), desc);
	        }

	        return _renderTargetView;
	    }

        D3D11.DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
	    {
	        return _depthStencilView;
	    }
    }
}
