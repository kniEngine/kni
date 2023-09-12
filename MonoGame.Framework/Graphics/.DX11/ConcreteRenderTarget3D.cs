// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTarget3D : ConcreteTexture3D, IRenderTarget3DStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTarget3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat)
            : base(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            PlatformConstructTexture3D_rt(contextStrategy, width, height, depth, mipMap, preferredSurfaceFormat);
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
        #endregion IRenderTarget2DStrategy



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
            texture3DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
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

            D3D11.Resource texture = new D3D11.Texture3D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

    }
}
