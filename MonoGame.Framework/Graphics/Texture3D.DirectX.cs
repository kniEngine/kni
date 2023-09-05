// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private void PlatformConstructTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            ((ConcreteTexture3D)_strategyTexture3D)._isRenderTarget = renderTarget;
            ((ConcreteTexture3D)_strategyTexture3D)._mipMap = mipMap;

            D3D11.Resource texture = CreateTexture();
            GetTextureStrategy<ConcreteTexture>()._texture = texture;
            GetTextureStrategy<ConcreteTexture>()._resourceView = new D3D11.ShaderResourceView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }


        internal override D3D11.Resource CreateTexture()
        {   
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture3DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (((ConcreteTexture3D)_strategyTexture3D)._isRenderTarget)
            {
                texture3DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
                if (((ConcreteTexture3D)_strategyTexture3D)._mipMap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    texture3DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
                }
            }

            return new D3D11.Texture3D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
        }

	    private void PlatformSetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                        T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture3D.SetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                        T[] data, int startIndex, int elementCount)
             where T : struct
        {
            _strategyTexture3D.GetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }
	}
}

