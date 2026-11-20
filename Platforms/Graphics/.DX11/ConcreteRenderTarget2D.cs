// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : ConcreteTexture2D, IRenderTarget2DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        private protected int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;

        private DXGI.SampleDescription _msSampleDescription;
        private protected D3D11.Texture2D _resolveTextureArray;
        private protected D3D11.RenderTargetView[] _renderTargetViews;
        private protected D3D11.DepthStencilView _depthStencilView;


        internal ConcreteRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
              TextureSurfaceType surfaceType)
            : base(contextStrategy, width, height, mipMap, preferredSurfaceFormat, arraySize, shared,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            if (surfaceType == TextureSurfaceType.RenderTargetSwapChain)
            {
                // Texture will be created by the RenderTargetSwapChain.
                return;
            }

            ConcreteGraphicsDevice graphicsDevice = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>();

            int maxMultiSampleCount = graphicsDevice.GetMaxMultiSampleCount(preferredSurfaceFormat);
            _multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTexture2D_rt(graphicsDevice.D3DDevice);

            PlatformConstructRenderTarget2D(graphicsDevice);
        }


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { return _depthStencilFormat; }
        }

        public int MultiSampleCount
        {
            get { return _multiSampleCount; }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { return _renderTargetUsage; }
        }

        public bool IsContentLost
        {
            get { return _isContentLost; }
        }
        #endregion IRenderTargetStrategy


        #region IRenderTargetStrategyDX11
        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView()
        {
            return _depthStencilView;
        }

        public virtual void ResolveSubresource(GraphicsContextStrategy graphicsContextStrategy, int arraySlice)
        {
            if (this.MultiSampleCount > 1)
            {
                lock (graphicsContextStrategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = graphicsContextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    int destinationSubresourceIndex = arraySlice * this.LevelCount;

                    System.Diagnostics.Debug.Assert(_resolveTextureArray != null);
                    d3dContext.ResolveSubresource(_resolveTextureArray, arraySlice, this.GetTexture(), destinationSubresourceIndex, this.Format.ToDXFormat());
                }
            }
        }
        #endregion IRenderTargetStrategyDX11


        private void PlatformConstructTexture2D_rt(D3D11.Device d3dDevice)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (_shared)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;
            if (MultiSampleCount <= 1 || _mipMap || _shared)
                texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            if (_mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(d3dDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(d3dDevice, texture);
        }

        private void PlatformConstructRenderTarget2D(ConcreteGraphicsDevice graphicsDevice)
        {
            _msSampleDescription = graphicsDevice.GetSupportedSampleDescription(this.Format.ToDXFormat(), _multiSampleCount);

            _renderTargetViews = new D3D11.RenderTargetView[this.ArraySize];

            if (_multiSampleCount > 1)
                CreateResolveTexture(graphicsDevice.D3DDevice);

            CreateRenderTargetView(graphicsDevice.D3DDevice);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencilView(graphicsDevice.D3DDevice);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice)
        {
            // Create one render target view per array slice.
            for (int i = 0; i < this.ArraySize; i++)
            {
                D3D11.Resource viewTex = _resolveTextureArray ?? _texture;

                System.Diagnostics.Debug.Assert(viewTex != null);

                if (this.ArraySize > 1)
                {
                    D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                    renderTargetViewDesc.Format = this.Format.ToDXFormat();
                    if (MultiSampleCount > 1)
                    {
                        renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DMultisampledArray;
                        renderTargetViewDesc.Texture2DMSArray.ArraySize = 1;
                        renderTargetViewDesc.Texture2DMSArray.FirstArraySlice = i;
                    }
                    else
                    {
                        renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
                        renderTargetViewDesc.Texture2DArray.ArraySize = 1;
                        renderTargetViewDesc.Texture2DArray.FirstArraySlice = i;
                        renderTargetViewDesc.Texture2DArray.MipSlice = 0;
                    }

                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex, renderTargetViewDesc);
                }
                else
                {
                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex);
                }
            }
        }

        private void CreateDepthStencilView(D3D11.Device d3dDevice)
        {
            // Create a descriptor for the depth/stencil buffer.
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Format = DepthStencilFormat.ToDXFormat();
            texture2DDesc.ArraySize = 1;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            // Allocate a 2-D surface as the depth/stencil buffer.
            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(d3dDevice, texture2DDesc))
            {
                D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                depthStencilViewDesc.Format = DepthStencilFormat.ToDXFormat();
                depthStencilViewDesc.Dimension = (MultiSampleCount > 1)
                                               ? D3D11.DepthStencilViewDimension.Texture2DMultisampled
                                               : D3D11.DepthStencilViewDimension.Texture2D;

                // Create a DepthStencil view on this surface to use on bind.
                _depthStencilView = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        private void CreateResolveTexture(D3D11.Device d3dDevice)
        {
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multi sampled texture.
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget; // ~BindFlags.ShaderResource, the multi sampled texture can never be bound directly.
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            _resolveTextureArray = new D3D11.Texture2D(d3dDevice, texture2DDesc);
        }

        protected override void PlatformGraphicsContextLost()
        {
            for (int i = 0; i < _renderTargetViews?.Length; i++)
                DX.Utilities.Dispose(ref _renderTargetViews[i]);

            DX.Utilities.Dispose(ref _resolveTextureArray);
            DX.Utilities.Dispose(ref _depthStencilView);

            base.PlatformGraphicsContextLost();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < _renderTargetViews?.Length; i++)
                    DX.Utilities.Dispose(ref _renderTargetViews[i]);

                DX.Utilities.Dispose(ref _resolveTextureArray);
                DX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }
    }
}
