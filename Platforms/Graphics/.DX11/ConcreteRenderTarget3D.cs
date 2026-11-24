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
    internal class ConcreteRenderTarget3D : ConcreteTexture3D, IRenderTarget3DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        private int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;

        private DXGI.SampleDescription _msSampleDescription;
        private D3D11.Texture2D _resolveTextureArray;
        private D3D11.Texture2D _resolveTextureSlice;
        private D3D11.RenderTargetView[] _renderTargetViews;
        private D3D11.DepthStencilView _depthStencilView;


        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            ConcreteGraphicsDevice graphicsDevice = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>();

            if (graphicsDevice.GraphicsProfile < GraphicsProfile.FL10_0)
                throw new NotSupportedException("RenderTarget3D on DX11 requires the FL10_0 profile or higher.");

            int maxMultiSampleCount = graphicsDevice.GetMaxMultiSampleCount(preferredSurfaceFormat);
            _multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTexture3D_rt(graphicsDevice.D3DDevice);

            PlatformConstructRenderTarget3D(graphicsDevice);
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

        public void ResolveSubresource(GraphicsContextStrategy graphicsContextStrategy, int arraySlice)
        {
            if (_resolveTextureArray == null)
                return;

            lock (graphicsContextStrategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = graphicsContextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                D3D11.ResourceRegion region = new D3D11.ResourceRegion(0, 0, 0, this.Width, this.Height, 1);

                System.Diagnostics.Debug.Assert(_resolveTextureArray != null);

                if (this.MultiSampleCount > 1)
                {
                    System.Diagnostics.Debug.Assert(_resolveTextureSlice != null);
                    // Resolve the multi-sampled texture slice to a non multi-sampled texture.
                    d3dContext.ResolveSubresource(_resolveTextureArray, arraySlice, _resolveTextureSlice, 0, this.Format.ToDXFormat());
                    // Copy the resolved texture slice to the 3D texture.
                    d3dContext.CopySubresourceRegion(_resolveTextureSlice, 0, region, GetTexture(), 0, 0, 0, arraySlice);
                }
                else
                {
                    // Copy the non multi-sampled texture slice directly to the 3D texture.
                    d3dContext.CopySubresourceRegion(_resolveTextureArray, arraySlice, region, GetTexture(), 0, 0, 0, arraySlice);
                }
            }
        }
        #endregion IRenderTargetStrategyDX11


        private void PlatformConstructTexture3D_rt(D3D11.Device d3dDevice)
        {
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = this.Format.ToDXFormat();
            texture3DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (MultiSampleCount <= 1 || _mipMap)
                texture3DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            if (_mipMap)
                texture3DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture3D(d3dDevice, texture3DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(d3dDevice, texture);
        }

        private void PlatformConstructRenderTarget3D(ConcreteGraphicsDevice graphicsDevice)
        {
            _msSampleDescription = graphicsDevice.GetSupportedSampleDescription(this.Format.ToDXFormat(), _multiSampleCount);

            _renderTargetViews = new D3D11.RenderTargetView[this.Depth];

            if (_multiSampleCount > 1 || _depthStencilFormat != DepthFormat.None)
                CreateResolveTextures(graphicsDevice.D3DDevice);

            CreateRenderTargetView(graphicsDevice.D3DDevice);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencilView(graphicsDevice.D3DDevice);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice)
        {
            // Create one render target view per depth slice.
            for (int i = 0; i < this.Depth; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Format = this.Format.ToDXFormat();
                if (MultiSampleCount > 1 || _depthStencilFormat != DepthFormat.None)
                {
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

                    System.Diagnostics.Debug.Assert(_resolveTextureArray != null);
                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, _resolveTextureArray, renderTargetViewDesc);
                }
                else
                {
                    renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture3D;
                    renderTargetViewDesc.Texture3D.DepthSliceCount = 1;
                    renderTargetViewDesc.Texture3D.FirstDepthSlice = i;
                    renderTargetViewDesc.Texture3D.MipSlice = 0;

                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, this.GetTexture(), renderTargetViewDesc);
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

        private void CreateResolveTextures(D3D11.Device d3dDevice)
        {
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multi sampled texture.
            texture2DDesc.ArraySize = this.Depth;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget; // ~BindFlags.ShaderResource, the multi sampled texture can never be bound directly.
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            _resolveTextureArray = new D3D11.Texture2D(d3dDevice, texture2DDesc);

            if (MultiSampleCount > 1)
            {
                DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
                texture2DDesc = new D3D11.Texture2DDescription();
                texture2DDesc.Width = this.Width;
                texture2DDesc.Height = this.Height;
                texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multi sampled texture.
                texture2DDesc.ArraySize = 1;
                texture2DDesc.Format = this.Format.ToDXFormat();
                texture2DDesc.BindFlags = D3D11.BindFlags.None;
                texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                texture2DDesc.SampleDescription = sampleDesc;
                texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

                _resolveTextureSlice = new D3D11.Texture2D(d3dDevice, texture2DDesc);
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            for (int i = 0; i < _renderTargetViews?.Length; i++)
                DX.Utilities.Dispose(ref _renderTargetViews[i]);

            DX.Utilities.Dispose(ref _resolveTextureArray);
            DX.Utilities.Dispose(ref _resolveTextureSlice);
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
                DX.Utilities.Dispose(ref _resolveTextureSlice);
                DX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }
    }
}
