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
using WIC = SharpDX.WIC;
using StbImageSharp;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        static ImagingFactory imgfactory_DX;

        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
                stream.Seek(0, SeekOrigin.Begin);

#if WINDOWS || WINDOWS_UAP
            if (stream.CanSeek)
                return PlatformFromStream_DX(graphicsDevice, stream);
#endif
            byte[] bytes;

            // Copy it's data to memory
            // As some platforms dont provide full stream functionality and thus streams can't be read as it is
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            // The data returned is always four channel BGRA
            ImageResult result = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, result.Width, result.Height);
            texture.SetData(result.Data);

            return texture;
        }

        private static BitmapSource LoadBitmap_DX(Stream stream, out WIC.BitmapDecoder decoder)
        {
            if (imgfactory_DX == null)
                imgfactory_DX = new ImagingFactory();

            decoder = new WIC.BitmapDecoder(
                imgfactory_DX,
                stream,
                DecodeOptions.CacheOnDemand
                );

            FormatConverter fconv = new FormatConverter(imgfactory_DX);

            using (BitmapFrameDecode frame = decoder.GetFrame(0))
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

            WIC.BitmapDecoder decoder;
            using (BitmapSource bmpSource = LoadBitmap_DX(stream, out decoder))
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
                using (DX.DataStream dataStream = new DX.DataStream(bmpSource.Size.Height * bmpSource.Size.Width * 4, true, true))
                {
                    bmpSource.CopyPixels(bmpSource.Size.Width * 4, dataStream);
                    DX.DataRectangle rect = new DX.DataRectangle(dataStream.DataPointer, bmpSource.Size.Width * 4);
                    textureResource = new D3D11.Texture2D(graphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc, rect);
                }
                texture.GetTextureStrategy<ConcreteTexture>().SetTextureInternal_DX(textureResource);

                return texture;
            }
        }
    }
}

