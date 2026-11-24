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
    internal class ConcreteRenderTargetCube : ConcreteTextureCube, IRenderTargetCubeStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        private int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;

        private DXGI.SampleDescription _msSampleDescription;
        private D3D11.Texture2D _resolveTextureArray;
        private D3D11.Texture2D[] _resolveTextures;
        private D3D11.RenderTargetView[] _renderTargetViews;
        private D3D11.DepthStencilView _depthStencilView;


        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, size, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            ConcreteGraphicsDevice graphicsDevice = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>();

            int maxMultiSampleCount = graphicsDevice.GetMaxMultiSampleCount(preferredSurfaceFormat);
            _multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTextureCube_rt(graphicsDevice.D3DDevice);

            PlatformConstructRenderTargetCube(graphicsDevice);
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

                    D3D11.Texture2D resolveTexture = _resolveTextureArray ?? _resolveTextures?[arraySlice];
                    int sourceSubresourceIndex = (_resolveTextureArray != null) ? arraySlice : 0;
                    int destinationSubresourceIndex = arraySlice * this.LevelCount;

                    System.Diagnostics.Debug.Assert(resolveTexture != null);
                    d3dContext.ResolveSubresource(resolveTexture, sourceSubresourceIndex, this.GetTexture(), destinationSubresourceIndex, this.Format.ToDXFormat());
                }
            }
        }
        #endregion IRenderTargetStrategyDX11


        private void PlatformConstructTextureCube_rt(D3D11.Device d3dDevice)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

            if (MultiSampleCount <= 1 || _mipMap)
                texture2DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
            if (_mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(d3dDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(d3dDevice, texture);
        }

        private void PlatformConstructRenderTargetCube(ConcreteGraphicsDevice graphicsDevice)
        {
            _msSampleDescription = graphicsDevice.GetSupportedSampleDescription(this.Format.ToDXFormat(), _multiSampleCount);

            _renderTargetViews = new D3D11.RenderTargetView[6];

            if (_multiSampleCount > 1)
                CreateResolveTextures(graphicsDevice.D3DDevice, graphicsDevice.GraphicsProfile);

            CreateRenderTargetView(graphicsDevice.D3DDevice, graphicsDevice.GraphicsProfile);
            if (DepthStencilFormat != DepthFormat.None)
                CreateDepthStencilView(graphicsDevice.D3DDevice, graphicsDevice.GraphicsProfile);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice, GraphicsProfile graphicsProfile)
        {
            // Create one render target view per cube map face.
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Format = this.Format.ToDXFormat();

                if (MultiSampleCount > 1)
                {
                    if (graphicsProfile >= GraphicsProfile.FL10_0)
                    {
                        renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DMultisampledArray;
                        renderTargetViewDesc.Texture2DMSArray.ArraySize = 1;
                        renderTargetViewDesc.Texture2DMSArray.FirstArraySlice = i;

                        System.Diagnostics.Debug.Assert(_resolveTextureArray != null);
                        _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, _resolveTextureArray, renderTargetViewDesc);
                    }
                    else
                    {
                        renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DMultisampled;

                        System.Diagnostics.Debug.Assert(_resolveTextures?[i] != null);
                        _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, _resolveTextures[i], renderTargetViewDesc);
                    }
                }
                else
                {
                    renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
                    renderTargetViewDesc.Texture2DArray.ArraySize = 1;
                    renderTargetViewDesc.Texture2DArray.FirstArraySlice = i;
                    renderTargetViewDesc.Texture2DArray.MipSlice = 0;

                    _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, this.GetTexture(), renderTargetViewDesc);
                }
            }
        }

        private void CreateDepthStencilView(D3D11.Device d3dDevice, GraphicsProfile graphicsProfile)
        {
            // Create a descriptor for the depth/stencil buffer.
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Format = DepthStencilFormat.ToDXFormat();
            texture2DDesc.MipLevels = 1;
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            if (graphicsProfile == GraphicsProfile.FL10_0 &&
                _resolveTextureArray == null)
            {
                // for feature Level_10_0 only, when a RenderTargetView directly uses a TextureCube,
                // the depth buffer is required to be defined as TextureCube with six slices,
                // and the depth view is required to be Texture2DArray.

                texture2DDesc.ArraySize = 6;
                texture2DDesc.OptionFlags = (MultiSampleCount > 1)
                                          ? D3D11.ResourceOptionFlags.None
                                          : D3D11.ResourceOptionFlags.TextureCube;

                // Allocate a 2-D surface as the depth/stencil buffer.
                using (D3D11.Texture2D depthTextureArray = new D3D11.Texture2D(d3dDevice, texture2DDesc))
                {
                    D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                    depthStencilViewDesc.Format = DepthStencilFormat.ToDXFormat();
                    if (MultiSampleCount > 1)
                    {
                        depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2DMultisampledArray;
                        depthStencilViewDesc.Texture2DMSArray.ArraySize = 1;
                        depthStencilViewDesc.Texture2DMSArray.FirstArraySlice = 0;
                    }
                    else
                    {
                        depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2DArray;
                        depthStencilViewDesc.Texture2DArray.ArraySize = 1;
                        depthStencilViewDesc.Texture2DArray.FirstArraySlice = 0;
                        depthStencilViewDesc.Texture2DArray.MipSlice = 0;
                    }

                    // Create a DepthStencil view on this surface to use on bind.
                    _depthStencilView = new D3D11.DepthStencilView(d3dDevice, depthTextureArray, depthStencilViewDesc);
                }
            }
            else
            {
                // for feature Level_10_1+ and Level_9.x, always use a single Texture2D depth buffer as it is valid with a TextureCube.
                // for feature Level_10.0, only use a single Texture2D depth buffer when a resolve texture is used.

                texture2DDesc.ArraySize = 1;

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
        }

        private void CreateResolveTextures(D3D11.Device d3dDevice, GraphicsProfile graphicsProfile)
        {
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = 1; // mip mapping is applied to the resolved texture, not the multi sampled texture.
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget; // ~BindFlags.ShaderResource, the multi sampled texture can never be bound directly.
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = _msSampleDescription;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (graphicsProfile >= GraphicsProfile.FL10_0)
            {
                texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
                _resolveTextureArray = new D3D11.Texture2D(d3dDevice, texture2DDesc);
            }
            else
            {
                texture2DDesc.ArraySize = 1;
                _resolveTextures = new D3D11.Texture2D[6];
                for (int i = 0; i < _resolveTextures.Length; i++)
                {
                    _resolveTextures[i] = new D3D11.Texture2D(d3dDevice, texture2DDesc);
                }
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            for (int i = 0; i < _renderTargetViews?.Length; i++)
                DX.Utilities.Dispose(ref _renderTargetViews[i]);

            DX.Utilities.Dispose(ref _resolveTextureArray);

            for (int i = 0; i < _resolveTextures?.Length; i++)
                DX.Utilities.Dispose(ref _resolveTextures[i]);

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

                for (int i = 0; i < _resolveTextures?.Length; i++)
                    DX.Utilities.Dispose(ref _resolveTextures[i]);

                DX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }
    }
}
