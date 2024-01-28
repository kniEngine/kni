// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget2D : ConcreteTexture2D, IRenderTarget2DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;


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

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.PresentationParameters.BackBufferFormat);
            this._multiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);


            PlatformConstructTexture2D_rt(contextStrategy, width, height, mipMap, preferredSurfaceFormat, shared);

            PlatformConstructRenderTarget2D(contextStrategy, width, height, mipMap, preferredDepthFormat, _multiSampleCount, shared);
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
            get
            {
                throw new NotImplementedException("IsContentLost");
                return _isContentLost;
            }
        }

        public virtual void ResolveSubresource(GraphicsContextStrategy contextStrategy)
        {
            lock (contextStrategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = contextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                System.Diagnostics.Debug.Assert(_msTexture != null);

                d3dContext.ResolveSubresource(
                    _msTexture,
                    0,
                    this.GetTexture(),
                    0,
                    this.Format.ToDXFormat());
            }
        }
        #endregion IRenderTargetStrategy


        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;


        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[0];
        }


        private void PlatformConstructTexture2D_rt(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
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
            if (MultiSampleCount == 0 || _shared)
                texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            if (_mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

        internal D3D11.Texture2D _msTexture;
        private DXGI.SampleDescription _msSampleDescription;

        private void PlatformConstructRenderTarget2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap,
            DepthFormat preferredDepthFormat, int multiSampleCount, bool shared)
        {
            D3D11.Device d3dDevice = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            _msSampleDescription = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetSupportedSampleDescription(this.Format.ToDXFormat(), multiSampleCount);

            _renderTargetViews = new D3D11.RenderTargetView[this.ArraySize];
            _depthStencilViews = new D3D11.DepthStencilView[1];

            if (multiSampleCount > 1)
                _msTexture = CreateMSTexture();

            CreateRenderTargetView(d3dDevice, width, height);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencilView(d3dDevice, width, height);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice, int width, int height)
        {
            D3D11.Resource viewTex = (MultiSampleCount > 1)
                                   ? _msTexture
                                   : _texture;

            System.Diagnostics.Debug.Assert(viewTex != null);

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
            texture2DDesc.Format = DepthStencilFormat.ToDXFormat();
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
                depthStencilViewDesc.Format = DepthStencilFormat.ToDXFormat();
                depthStencilViewDesc.Dimension = (MultiSampleCount > 1)
                                               ? D3D11.DepthStencilViewDimension.Texture2DMultisampled
                                               : D3D11.DepthStencilViewDimension.Texture2D;
                _depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        private D3D11.Texture2D CreateMSTexture()
        {
            System.Diagnostics.Debug.Assert(_msTexture == null);

            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multisampled texture;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget; // ~BindFlags.ShaderResource, the multi sampled texture can never be bound directly.
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            return new D3D11.Texture2D(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }

        protected override void PlatformGraphicsContextLost()
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

            base.PlatformGraphicsContextLost();
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


    }
}
