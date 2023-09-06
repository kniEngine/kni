// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

#if WINDOWS_UAP
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            ((ConcreteTexture2D)_strategyTexture2D)._shared = shared;
            ((ConcreteTexture2D)_strategyTexture2D)._mipMap = mipMap;

            D3D11.Resource texture = CreateTexture(contextStrategy);
            GetTextureStrategy<ConcreteTexture>()._texture = texture;
            GetTextureStrategy<ConcreteTexture>()._resourceView = new D3D11.ShaderResourceView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

        protected override D3D11.Resource CreateTexture(GraphicsContextStrategy contextStrategy)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (((ConcreteTexture2D)_strategyTexture2D)._shared)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;

            return new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
        }
    }
}

