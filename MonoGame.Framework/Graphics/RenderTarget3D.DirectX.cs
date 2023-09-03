// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget3D : IRenderTargetDX11
    {

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
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
            texture2DDesc.ArraySize = 1;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.Width = width;
            texture2DDesc.Height = height;
            texture2DDesc.SampleDescription = multisampleDesc;
            texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc))
            {
                D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
                // Create the view for binding to the device.
                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilView = new D3D11.DepthStencilView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView);
                DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilView);
            }

            base.Dispose(disposing);
        }

        D3D11.RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
	    {
            if (arraySlice >= Depth)
                throw new ArgumentOutOfRangeException("The arraySlice is out of range for this Texture3D.");

            // Dispose the previous target.
	        if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._currentSlice != arraySlice && ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView != null)
	        {
                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView.Dispose();
                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView = null;
	        }

            // Create the new target view interface.
	        if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView == null)
	        {
                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._currentSlice = arraySlice;

                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
                renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture3D;
                renderTargetViewDesc.Texture3D.DepthSliceCount = -1;
                renderTargetViewDesc.Texture3D.FirstDepthSlice = arraySlice;
                renderTargetViewDesc.Texture3D.MipSlice = 0;

                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView = new D3D11.RenderTargetView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, GetTexture(), renderTargetViewDesc);
	        }

	        return ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetView;
	    }

        D3D11.DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
	    {
	        return ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilView;
	    }
    }
}
