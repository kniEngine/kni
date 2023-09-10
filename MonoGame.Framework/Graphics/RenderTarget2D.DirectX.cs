// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTarget2D
    {
        private D3D11.Texture2D _msTexture;
        private DXGI.SampleDescription _msSampleDescription;

        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            base.PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, bool shared)
        {
            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            D3D11.Device d3dDevice = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            _msSampleDescription = contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(GraphicsExtensions.ToDXFormat(this.Format), this.MultiSampleCount);

            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews = new D3D11.RenderTargetView[this.ArraySize];
            ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews = new D3D11.DepthStencilView[1];

            CreateRenderTargetView(d3dDevice, width, height);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencilView(d3dDevice, width, height);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice, int width, int height)
        {
            D3D11.Resource viewTex = (MultiSampleCount > 1)
                                   ? GetMSTexture()
                                   : this.GetTextureStrategy<ConcreteTexture>().GetTexture();

            // Create a view interface on the rendertarget to use on bind.
            if (this.ArraySize > 1)
            {
                for (int i = 0; i < this.ArraySize; i++)
                {
                    D3D11.RenderTargetViewDescription renderTargetViewDescription = new D3D11.RenderTargetViewDescription();
                    if (MultiSampleCount > 1)
                    {
                        renderTargetViewDescription.Dimension = D3D11.RenderTargetViewDimension.Texture2DMultisampledArray;
                        renderTargetViewDescription.Texture2DMSArray.ArraySize = 1;
                        renderTargetViewDescription.Texture2DMSArray.FirstArraySlice = i;
                    }
                    else
                    {
                        renderTargetViewDescription.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
                        renderTargetViewDescription.Texture2DArray.ArraySize = 1;
                        renderTargetViewDescription.Texture2DArray.FirstArraySlice = i;
                        renderTargetViewDescription.Texture2DArray.MipSlice = 0;
                    }
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex, renderTargetViewDescription);
                }
            }
            else
            {
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews[0] = new D3D11.RenderTargetView(d3dDevice, viewTex);
            }
        }

        private void CreateDepthStencilView(D3D11.Device d3dDevice, int width, int height)
        {
            // The depth stencil view's multisampling configuration must strictly
            // match the texture's multisampling configuration.  Ignore whatever parameters
            // were provided and use the texture's configuration so that things are
            // guarenteed to work.
            DXGI.SampleDescription multisampleDesc = _msSampleDescription;

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(DepthStencilFormat);
            texture2DDesc.ArraySize = 1;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.Width = width;
            texture2DDesc.Height = height;
            texture2DDesc.SampleDescription = multisampleDesc;
            texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(d3dDevice, texture2DDesc))
            {
                // Create the view for binding to the device.
                D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(DepthStencilFormat);
                depthStencilViewDesc.Dimension = (MultiSampleCount > 1)
                                               ? D3D11.DepthStencilViewDimension.Texture2DMultisampled
                                               : D3D11.DepthStencilViewDimension.Texture2D;
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews != null)
            {
                for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews.Length; i++)
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews[i].Dispose();
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews = null;
            }
            if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews != null)
            {
                for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews.Length; i++)
                    DX.Utilities.Dispose(ref ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews[i]);
                ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews.Length; i++)
                        ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews[i].Dispose();
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._renderTargetViews = null;
                }
                if (((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews.Length; i++)
                        DX.Utilities.Dispose(ref ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews[i]);
                    ((ConcreteRenderTarget2D)_strategyRenderTarget2D)._depthStencilViews = null;
                }
                DX.Utilities.Dispose(ref _msTexture);
            }

            base.Dispose(disposing);
        }

        internal virtual void ResolveSubresource()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.ResolveSubresource(
                    GetMSTexture(),
                    0,
                    this.GetTextureStrategy<ConcreteTexture>().GetTexture(),
                    0,
                    GraphicsExtensions.ToDXFormat(this.Format));
            }
        }

        protected override D3D11.Resource CreateTexture(GraphicsContextStrategy contextStrategy)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (((ConcreteTexture2D)_strategyTexture2D)._shared)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;
            if (MultiSampleCount == 0 || ((ConcreteTexture2D)_strategyTexture2D)._shared)
                texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            if (((ConcreteTexture2D)_strategyTexture2D)._mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            return new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }

        private D3D11.Texture2D GetMSTexture()
        {
            if (_msTexture != null)
                return _msTexture;
            
            _msTexture = CreateMSTexture();
            return _msTexture;
        }

        internal virtual D3D11.Texture2D CreateMSTexture()
        {
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multisampled texture;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget; // ~BindFlags.ShaderResource, the multi sampled texture can never be bound directly.
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (((ConcreteTexture2D)_strategyTexture2D)._shared)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;

            return new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }

    }
}
