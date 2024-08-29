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
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;


        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.PresentationParameters.BackBufferFormat);
            this._multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTexture3D_rt(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat);

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat != DepthFormat.None)
                PlatformConstructRenderTarget3D(contextStrategy, width, height, depth, mipMap, preferredDepthFormat, _multiSampleCount);
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

        public void ResolveSubresource(GraphicsContextStrategy graphicsContextStrategy)
        {
            if (this.MultiSampleCount > 1)
            {
            }
        }
        #endregion IRenderTargetStrategy



        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;
        internal int _currentSlice;

        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
	    {
	        return _renderTargetViews[arraySlice];
	    }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView(int arraySlice)
	    {
	        return _depthStencilViews[0];
	    }


        private void PlatformConstructTexture3D_rt(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = this.Format.ToDXFormat();
            texture3DDesc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (_mipMap)
            {
                // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                // because generation of mipmaps is not supported on the Xbox 360.
                // TODO: New method Texture.GenerateMipMaps() required.
                texture3DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
            }

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture3D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

        private void PlatformConstructRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap,
            DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            _renderTargetViews = new D3D11.RenderTargetView[depth];
            _depthStencilViews = new D3D11.DepthStencilView[1];

            // Setup the multisampling description.
            DXGI.SampleDescription multisampleDesc = new DXGI.SampleDescription(1, 0);
            if (multiSampleCount > 1)
            {
                multisampleDesc.Count = multiSampleCount;
                multisampleDesc.Quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            D3D11.Device d3dDevice = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice;

            CreateRenderTargetView(d3dDevice, width, height, depth);
            CreateDepthStencilView(d3dDevice, width, height, depth, preferredDepthFormat, multisampleDesc);
        }

        private void CreateRenderTargetView(D3D11.Device d3dDevice, int width, int height, int depth)
        {
            D3D11.Resource viewTex = this.GetTexture();

            for (int i = 0; i < depth; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Format = this.Format.ToDXFormat();
                renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture3D;
                renderTargetViewDesc.Texture3D.DepthSliceCount = -1;
                renderTargetViewDesc.Texture3D.FirstDepthSlice = i;
                renderTargetViewDesc.Texture3D.MipSlice = 0;

                _renderTargetViews[i] = new D3D11.RenderTargetView(d3dDevice, viewTex, renderTargetViewDesc);
            }
        }

        private void CreateDepthStencilView(D3D11.Device d3dDevice, int width, int height, int depth, DepthFormat preferredDepthFormat, DXGI.SampleDescription multisampleDesc)
        {

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Format = preferredDepthFormat.ToDXFormat();
            texture2DDesc.ArraySize = 1;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.Width = width;
            texture2DDesc.Height = height;
            texture2DDesc.SampleDescription = multisampleDesc;
            texture2DDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            using (D3D11.Texture2D depthBuffer = new D3D11.Texture2D(d3dDevice, texture2DDesc))
            {
                D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                depthStencilViewDesc.Format = preferredDepthFormat.ToDXFormat();
                depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
                // Create the view for binding to the device.
                _depthStencilViews[0] = new D3D11.DepthStencilView(d3dDevice, depthBuffer, depthStencilViewDesc);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_renderTargetViews != null)
                {
                    for (int i = 0; i < _renderTargetViews.Length; i++)
                        DX.Utilities.Dispose(ref _renderTargetViews[i]);
                    _renderTargetViews = null;
                }
                if (_depthStencilViews != null)
                {
                    for (int i = 0; i < _depthStencilViews.Length; i++)
                        DX.Utilities.Dispose(ref _depthStencilViews[i]);
                    _depthStencilViews = null;
                }                
            }

            base.Dispose(disposing);
        }
    }
}
