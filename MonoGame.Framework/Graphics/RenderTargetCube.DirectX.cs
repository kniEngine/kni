// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetDX11
    {
        private SharpDX.Direct3D11.Resource _depthTarget;
        private RenderTargetView[] _renderTargetViews;
        private DepthStencilView[] _depthStencilViews;

        private void PlatformConstructRenderTargetCube(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.Strategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);
            RenderTargetUsage = usage;

            _renderTargetViews = new RenderTargetView[6];
            _depthStencilViews = new DepthStencilView[6];

            // Create one render target view per cube map face.          
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                var renderTargetViewDesc = new RenderTargetViewDescription();
                renderTargetViewDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                renderTargetViewDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
                renderTargetViewDesc.Texture2DArray.ArraySize = 1;
                renderTargetViewDesc.Texture2DArray.FirstArraySlice = i;
                renderTargetViewDesc.Texture2DArray.MipSlice = 0;
                _renderTargetViews[i] = new RenderTargetView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, GetTexture(), renderTargetViewDesc);
            }


            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

            DXGI.SampleDescription sampleDescription = new DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                sampleDescription.Count = MultiSampleCount;
                sampleDescription.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var depthStencilDesc = new Texture2DDescription();
            depthStencilDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
            depthStencilDesc.ArraySize = 1;
            depthStencilDesc.MipLevels = 1;
            depthStencilDesc.Width = this.Size;
            depthStencilDesc.Height = this.Size;
            depthStencilDesc.SampleDescription = sampleDescription;
            depthStencilDesc.BindFlags = BindFlags.DepthStencil;

            if (graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice.FeatureLevel >= SharpDX.Direct3D.FeatureLevel.Level_10_0)
            {
                // for feature Level_10_0 the depth buffer is required to be defined as TextureCube with six slices,
                // and the depth view is required to be Texture2DArray.

                depthStencilDesc.ArraySize = 6;
                depthStencilDesc.OptionFlags = ResourceOptionFlags.TextureCube;

                _depthTarget = new SharpDX.Direct3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                for (int i = 0; i < _renderTargetViews.Length; i++)
                {
                    var depthStencilViewDesc = new DepthStencilViewDescription();
                    depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                    depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                    depthStencilViewDesc.Texture2DArray.ArraySize = 1;
                    depthStencilViewDesc.Texture2DArray.FirstArraySlice = i;
                    _depthStencilViews[i] = new DepthStencilView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _depthTarget, depthStencilViewDesc);
                }
            }
            else
            {
                // Create one depth target view per cube map face.
                // feature Level_9_x doesn't support Texture2DArray.

                for (int i = 0; i < _renderTargetViews.Length; i++)
                {
                    _depthTarget = new SharpDX.Direct3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                    var depthStencilViewDesc = new DepthStencilViewDescription();
                    depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                    depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
                    _depthStencilViews[i] = new DepthStencilView(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _depthTarget, depthStencilViewDesc);
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
                    for (var i = 0; i < _renderTargetViews.Length; i++)
                        _renderTargetViews[i].Dispose();
                    _renderTargetViews = null;
                }

                if (_depthStencilViews != null)
                {
                    for (var i = 0; i < _depthStencilViews.Length; i++)
                        if (_depthStencilViews[i] != null)
                            _depthStencilViews[i].Dispose();
                    _depthStencilViews = null;
                }

                SharpDX.Utilities.Dispose(ref _depthTarget);
            }

            base.Dispose(disposing);
        }

        RenderTargetView IRenderTargetDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        DepthStencilView IRenderTargetDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[arraySlice];
        }
    }
}
