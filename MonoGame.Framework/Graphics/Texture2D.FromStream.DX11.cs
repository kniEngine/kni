// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2019 Kastellanos Nikos

using System.IO;
using Microsoft.Xna.Platform.Graphics;
using SharpDX.WIC;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        static ImagingFactory imgfactory_DX;
        
        private static BitmapSource LoadBitmap_DX(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory_DX == null)
            {
                imgfactory_DX = new ImagingFactory();
            }

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory_DX,
                stream,
                DecodeOptions.CacheOnDemand
                );

            var fconv = new FormatConverter(imgfactory_DX);

            using (var frame = decoder.GetFrame(0))
            {
                fconv.Initialize(
                    frame,
                    PixelFormat.Format32bppRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);
            }
            return fconv;
        }

        private unsafe static Texture2D PlatformFromStream_DX(GraphicsDevice graphicsDevice, Stream stream)
        {
            // For reference this implementation was ultimately found through this post:
            // http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-metro 
            
            SharpDX.WIC.BitmapDecoder decoder;
            using (var bmpSource = LoadBitmap_DX(stream, out decoder))
            using (decoder)
            {
                Texture2D texture = new Texture2D(graphicsDevice, bmpSource.Size.Width, bmpSource.Size.Height);

                // TODO: use texture.SetData(...)
                D3D11.Texture2DDescription texture2DDesc;
                texture2DDesc.Width = bmpSource.Size.Width;
                texture2DDesc.Height = bmpSource.Size.Height;
                texture2DDesc.ArraySize = 1;
                texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
                texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                texture2DDesc.Format = DXGI.Format.R8G8B8A8_UNorm;
                texture2DDesc.MipLevels = 1;
                texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
                texture2DDesc.SampleDescription.Count = 1;
                texture2DDesc.SampleDescription.Quality = 0;

                D3D11.Texture2D textureResource;
                using (DX.DataStream s = new DX.DataStream(bmpSource.Size.Height * bmpSource.Size.Width * 4, true, true))
                {
                    bmpSource.CopyPixels(bmpSource.Size.Width * 4, s);
                    DX.DataRectangle rect = new DX.DataRectangle(s.DataPointer, bmpSource.Size.Width * 4);
                    textureResource = new D3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc, rect);
                }
                texture.GetTextureStrategy<ConcreteTexture>().SetTextureInternal_DX(textureResource);

                return texture;
            }
        }
    }
}

