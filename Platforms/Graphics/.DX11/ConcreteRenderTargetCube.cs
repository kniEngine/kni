﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetCube : ConcreteTextureCube, IRenderTargetCubeStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;


        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, size, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(preferredSurfaceFormat);
            this._multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTextureCube_rt(contextStrategy, size, mipMap, preferredSurfaceFormat);

            PlatformConstructRenderTargetCube(contextStrategy, mipMap, preferredDepthFormat, _multiSampleCount);
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

        public void ResolveSubresource(GraphicsContextStrategy graphicsContextStrategy)
        {
            if (this.MultiSampleCount > 1)
            {
            }
        }
        #endregion IRenderTargetStrategy


        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;
        internal D3D11.Resource _depthTarget;


        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[arraySlice];
        }


        private void PlatformConstructTextureCube_rt(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

            if (_mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            _renderTargetViews = new D3D11.RenderTargetView[6];
            _depthStencilViews = new D3D11.DepthStencilView[6];

            // Create one render target view per cube map face.          
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
                renderTargetViewDesc.Format = this.Format.ToDXFormat();
                renderTargetViewDesc.Texture2DArray.ArraySize = 1;
                renderTargetViewDesc.Texture2DArray.FirstArraySlice = i;
                renderTargetViewDesc.Texture2DArray.MipSlice = 0;
                _renderTargetViews[i] = new D3D11.RenderTargetView(
                    ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice,
                    this.GetTexture(),
                    renderTargetViewDesc);
            }


            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

            DXGI.SampleDescription sampleDescription = new DXGI.SampleDescription(1, 0);
            if (multiSampleCount > 1)
            {
                sampleDescription.Count = multiSampleCount;
                sampleDescription.Quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            D3D11.Texture2DDescription depthStencilDesc = new D3D11.Texture2DDescription();
            depthStencilDesc.Format = preferredDepthFormat.ToDXFormat();
            depthStencilDesc.ArraySize = 1;
            depthStencilDesc.MipLevels = 1;
            depthStencilDesc.Width = this.Size;
            depthStencilDesc.Height = this.Size;
            depthStencilDesc.SampleDescription = sampleDescription;
            depthStencilDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            if (((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice.FeatureLevel >= D3D.FeatureLevel.Level_10_0)
            {
                // for feature Level_10_0 the depth buffer is required to be defined as TextureCube with six slices,
                // and the depth view is required to be Texture2DArray.

                depthStencilDesc.ArraySize = 6;
                depthStencilDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

                _depthTarget = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                for (int i = 0; i < _renderTargetViews.Length; i++)
                {
                    D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                    depthStencilViewDesc.Format = preferredDepthFormat.ToDXFormat();
                    depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2DArray;
                    depthStencilViewDesc.Texture2DArray.ArraySize = 1;
                    depthStencilViewDesc.Texture2DArray.FirstArraySlice = i;
                    _depthStencilViews[i] = new D3D11.DepthStencilView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _depthTarget, depthStencilViewDesc);
                }
            }
            else
            {
                // Create one depth target view per cube map face.
                // feature Level_9_x doesn't support Texture2DArray.

                for (int i = 0; i < _renderTargetViews.Length; i++)
                {
                    _depthTarget = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                    D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                    depthStencilViewDesc.Format = preferredDepthFormat.ToDXFormat();
                    depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
                    _depthStencilViews[i] = new D3D11.DepthStencilView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _depthTarget, depthStencilViewDesc);
                }
            }

            return;
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
                        if (_depthStencilViews[i] != null)
                            _depthStencilViews[i].Dispose();
                    _depthStencilViews = null;
                }

                DX.Utilities.Dispose(ref _depthTarget);
            }

            base.Dispose(disposing);
        }
    }
}
