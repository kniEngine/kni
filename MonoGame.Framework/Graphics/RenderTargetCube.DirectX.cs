// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube
    {
        private void PlatformConstructTextureCube_rt(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            D3D11.Resource texture = CreateTexture(contextStrategy);
            GetTextureStrategy<ConcreteTexture>()._texture = texture;
            GetTextureStrategy<ConcreteTexture>()._resourceView = new D3D11.ShaderResourceView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);

        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._multiSampleCount = contextStrategy.Context.DeviceStrategy.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount);

            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews = new D3D11.RenderTargetView[6];
            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews = new D3D11.DepthStencilView[6];

            // Create one render target view per cube map face.          
            for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews.Length; i++)
            {
                D3D11.RenderTargetViewDescription renderTargetViewDesc = new D3D11.RenderTargetViewDescription();
                renderTargetViewDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
                renderTargetViewDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
                renderTargetViewDesc.Texture2DArray.ArraySize = 1;
                renderTargetViewDesc.Texture2DArray.FirstArraySlice = i;
                renderTargetViewDesc.Texture2DArray.MipSlice = 0;
                ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews[i] = new D3D11.RenderTargetView(
                    contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice,
                    this.GetTextureStrategy<ConcreteTexture>().GetTexture(),
                    renderTargetViewDesc);
            }


            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

            DXGI.SampleDescription sampleDescription = new DXGI.SampleDescription(1, 0);
            if (MultiSampleCount > 1)
            {
                sampleDescription.Count = MultiSampleCount;
                sampleDescription.Quality = (int)D3D11.StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            D3D11.Texture2DDescription depthStencilDesc = new D3D11.Texture2DDescription();
            depthStencilDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
            depthStencilDesc.ArraySize = 1;
            depthStencilDesc.MipLevels = 1;
            depthStencilDesc.Width = this.Size;
            depthStencilDesc.Height = this.Size;
            depthStencilDesc.SampleDescription = sampleDescription;
            depthStencilDesc.BindFlags = D3D11.BindFlags.DepthStencil;

            if (contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice.FeatureLevel >= D3D.FeatureLevel.Level_10_0)
            {
                // for feature Level_10_0 the depth buffer is required to be defined as TextureCube with six slices,
                // and the depth view is required to be Texture2DArray.

                depthStencilDesc.ArraySize = 6;
                depthStencilDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

                ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget = new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews.Length; i++)
                {
                    D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                    depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                    depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2DArray;
                    depthStencilViewDesc.Texture2DArray.ArraySize = 1;
                    depthStencilViewDesc.Texture2DArray.FirstArraySlice = i;
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i] = new D3D11.DepthStencilView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget, depthStencilViewDesc);
                }
            }
            else
            {
                // Create one depth target view per cube map face.
                // feature Level_9_x doesn't support Texture2DArray.

                for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews.Length; i++)
                {
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget = new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, depthStencilDesc);

                    D3D11.DepthStencilViewDescription depthStencilViewDesc = new D3D11.DepthStencilViewDescription();
                    depthStencilViewDesc.Format = GraphicsExtensions.ToDXFormat(preferredDepthFormat);
                    depthStencilViewDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i] = new D3D11.DepthStencilView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget, depthStencilViewDesc);
                }
            }

            return;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews.Length; i++)
                        ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews[i].Dispose();
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._renderTargetViews = null;
                }

                if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews != null)
                {
                    for (int i = 0; i < ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews.Length; i++)
                        if (((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i] != null)
                            ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews[i].Dispose();
                    ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthStencilViews = null;
                }

                DX.Utilities.Dispose(ref ((ConcreteRenderTargetCube)_strategyRenderTargetCube)._depthTarget);
            }

            base.Dispose(disposing);
        }


        protected override D3D11.Resource CreateTexture(GraphicsContextStrategy contextStrategy)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

            if (((ConcreteTextureCube)_strategyTextureCube)._mipMap)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
            
            return new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }
    }
}
