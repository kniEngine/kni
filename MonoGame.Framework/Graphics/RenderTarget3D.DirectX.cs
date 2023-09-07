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

        private void PlatformConstructRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews = new D3D11.RenderTargetView[depth];
            ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews = new D3D11.DepthStencilView[1];

            // Setup the multisampling description.
            DXGI.SampleDescription multisampleDesc = new DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                multisampleDesc.Count = MultiSampleCount;
                multisampleDesc.Quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            D3D11.Device d3dDevice = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            CreateRenderTargetView(d3dDevice, width, height, depth);
            CreateDepthStencilView(d3dDevice, width, height, depth, preferredDepthFormat, multisampleDesc);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice, int width, int height, int depth)
        {
            D3D11.Resource viewTex = this.GetTextureStrategy<ConcreteTexture>().GetTexture();

            for (int i = 0; i < depth; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
                renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture3D;
                renderTargetViewDesc.Texture3D.DepthSliceCount = -1;
                renderTargetViewDesc.Texture3D.FirstDepthSlice = i;
                renderTargetViewDesc.Texture3D.MipSlice = 0;

                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex, renderTargetViewDesc);
            }
        }

        private void CreateDepthStencilView(D3D11.Device d3dDevice, int width, int height, int depth, DepthFormat preferredDepthFormat, DXGI.SampleDescription multisampleDesc)
        {

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

            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(d3dDevice, texture2DDesc))
            {
                D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
                // Create the view for binding to the device.
                ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews.Length; i++)
                        DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews[i]);
                    ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews = null;
                }
                if (((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews.Length; i++)
                        DX.Utilities.Dispose(ref ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews[i]);
                    ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews = null;
                }                
            }

            base.Dispose(disposing);
        }

        D3D11.RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
	    {
	        return ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._renderTargetViews[arraySlice];
	    }

        D3D11.DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
	    {
	        return ((ConcreteRenderTarget3D)_strategyRenderTarget3D)._depthStencilViews[0];
	    }

        protected override D3D11.Resource CreateTexture(GraphicsContextStrategy contextStrategy)
        {
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture3DDesc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (((ConcreteTexture3D)_strategyTexture3D)._mipMap)
            {
                // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                // because generation of mipmaps is not supported on the Xbox 360.
                // TODO: New method Texture.GenerateMipMaps() required.
                texture3DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
            }

            return new D3D11.Texture3D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
        }

    }
}
