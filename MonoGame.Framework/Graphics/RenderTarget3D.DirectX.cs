// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D : IRenderTargetDX11
    {
        private int _currentSlice;
        private RenderTargetView _renderTargetView;
        private DepthStencilView _depthStencilView;

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
                multisampleDesc.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, new Texture2DDescription
            {
                Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = multisampleDesc,
                BindFlags = BindFlags.DepthStencil,
            }))
            {
                // Create the view for binding to the device.
                _depthStencilView = new DepthStencilView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthBuffer, new DepthStencilViewDescription()
                {
                    Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat),
                    Dimension = DepthStencilViewDimension.Texture2D
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

	    RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
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

	            var desc = new RenderTargetViewDescription
	            {
	                Format = GraphicsExtensions.ToDXFormat(this.Format),
	                Dimension = RenderTargetViewDimension.Texture3D,
	                Texture3D =
	                    {
	                        DepthSliceCount = -1,
	                        FirstDepthSlice = arraySlice,
	                        MipSlice = 0,
	                    }
	            };

	            _renderTargetView = new RenderTargetView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, GetTexture(), desc);
	        }

	        return _renderTargetView;
	    }

	    DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
	    {
	        return _depthStencilView;
	    }
    }
}
