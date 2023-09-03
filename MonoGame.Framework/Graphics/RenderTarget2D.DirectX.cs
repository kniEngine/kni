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
    public partial class RenderTarget2D : IRenderTargetDX11
    {
        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;

        private D3D11.Texture2D _msTexture;
        private DXGI.SampleDescription _msSampleDescription;

        private void PlatformConstructRenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);
            RenderTargetUsage = usage;

            D3D11.Device d3dDevice = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            _msSampleDescription = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(GraphicsExtensions.ToDXFormat(this.Format), this.MultiSampleCount);

            _renderTargetViews = new D3D11.RenderTargetView[this.ArraySize];
            _depthStencilViews = new D3D11.DepthStencilView[1];

            CreateRenderTarget(d3dDevice, width, height);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencil(d3dDevice, width, height);
        }

        private void CreateRenderTarget(D3D11.Device d3dDevice, int width, int height)
        {
            D3D11.Resource viewTex = MultiSampleCount > 1 ? GetMSTexture() : GetTexture();

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
                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex, renderTargetViewDescription);
                }
            }
            else
            {
                _renderTargetViews[0] = new D3D11.RenderTargetView(d3dDevice, viewTex);
            }
        }

        private void CreateDepthStencil(D3D11.Device d3dDevice, int width, int height)
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
                _depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (_renderTargetViews != null)
            {
                for (int i = 0; i < _renderTargetViews.Length; i++)
                    _renderTargetViews[i].Dispose();
                _renderTargetViews = null;
            }
            if (_depthStencilViews != null)
            {
                for (int i = 0; i < _depthStencilViews.Length; i++)
                    DX.Utilities.Dispose(ref _depthStencilViews[i]);
                _depthStencilViews = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_renderTargetViews != null)
                {
                    for (int i = 0; i < _renderTargetViews.Length; i++)
                        _renderTargetViews[i].Dispose();
                    _renderTargetViews = null;
                }
                if (_depthStencilViews != null)
                {
                    for (int i = 0; i < _depthStencilViews.Length; i++)
                        DX.Utilities.Dispose(ref _depthStencilViews[i]);
                    _depthStencilViews = null;
                }
                DX.Utilities.Dispose(ref _msTexture);
            }

            base.Dispose(disposing);
        }

        D3D11.RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[0];
        }

        internal virtual void ResolveSubresource()
        {
            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                d3dContext.ResolveSubresource(
                    GetMSTexture(),
                    0,
                    GetTexture(),
                    0,
                    GraphicsExtensions.ToDXFormat(this.Format));
            }
        }

        protected internal override D3D11.Texture2DDescription GetTexture2DDescription()
        {
            D3D11.Texture2DDescription texture2DDesc = base.GetTexture2DDescription();

            if (MultiSampleCount == 0 || Shared)
                texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;

            if (MipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            return texture2DDesc;
        }

        private D3D11.Texture2D GetMSTexture()
        {
            if (_msTexture == null)
                _msTexture = CreateMSTexture();

            return _msTexture;
        }

        internal virtual D3D11.Texture2D CreateMSTexture()
        {
            D3D11.Texture2DDescription texture2DDesc = GetMSTexture2DDescription();

            return new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }

        internal virtual D3D11.Texture2DDescription GetMSTexture2DDescription()
        {
            D3D11.Texture2DDescription texture2DDesc = base.GetTexture2DDescription();

            texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            // the multi sampled texture can never be bound directly
            texture2DDesc.BindFlags &= ~D3D11.BindFlags.ShaderResource;
            texture2DDesc.SampleDescription = _msSampleDescription;
            // mip mapping is applied to the resolved texture, not the multisampled texture
            texture2DDesc.MipLevels = 1;
            texture2DDesc.OptionFlags &= ~D3D11.ResourceOptionFlags.GenerateMipMaps;

            return texture2DDesc;
        }
    }
}
