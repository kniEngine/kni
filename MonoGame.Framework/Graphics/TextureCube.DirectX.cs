// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private void PlatformConstructTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
        {
            ((ConcreteTextureCube)_strategyTextureCube)._mipMap = mipMap;

            D3D11.Resource texture = CreateTexture();
            GetTextureStrategy<ConcreteTexture>()._texture = texture;
            GetTextureStrategy<ConcreteTexture>()._resourceView = new D3D11.ShaderResourceView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

        protected override D3D11.Resource CreateTexture()
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

            return new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }

	}
}

