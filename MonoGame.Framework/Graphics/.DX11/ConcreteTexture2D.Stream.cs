// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Graphics.Utilities.Png;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using WIC = SharpDX.WIC;

#if UAP || WINUI
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif


namespace Microsoft.Xna.Platform.Graphics
{
    internal partial class ConcreteTexture2D
    {
        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, Stream stream)
            : base(contextStrategy, SurfaceFormat.Color, 1)
        {
            this._arraySize = 1;
            
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
                stream.Seek(0, SeekOrigin.Begin);

            if (stream.CanSeek)
            {
                PlatformFromStream_DX(contextStrategy, stream, out this._width, out this._height);
            }
            else
            {
                // If stream doesn't provide seek functionality, use MemoryStream instead
                using (Stream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    PlatformFromStream_ImageSharp(contextStrategy, ms, out this._width, out this._height);
                 }
            }
        }

        private unsafe void PlatformFromStream_ImageSharp(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            // The data returned is always four channel BGRA
            StbImageSharp.ImageResult result = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            width = result.Width;
            height = result.Height;
            ValidateBounds(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy, width, height);

            this.PlatformConstructTexture2D(contextStrategy, width, height, false, SurfaceFormat.Color, false);
            this.SetData<byte>(0, result.Data, 0, result.Data.Length);
        }

        private static WIC.BitmapSource LoadBitmap_DX(Stream stream, out WIC.BitmapDecoder decoder)
        {
            using (WIC.ImagingFactory imgfactory = new WIC.ImagingFactory())
            {
                decoder = new WIC.BitmapDecoder(imgfactory, stream, WIC.DecodeOptions.CacheOnDemand);

                WIC.FormatConverter fconv = new WIC.FormatConverter(imgfactory);

                using (WIC.BitmapFrameDecode frame = decoder.GetFrame(0))
                {
                    fconv.Initialize(
                        frame,
                        WIC.PixelFormat.Format32bppRGBA,
                        WIC.BitmapDitherType.None,
                        null,
                        0.0,
                        WIC.BitmapPaletteType.Custom);
                }
                return fconv;
            }
        }

        private unsafe void PlatformFromStream_DX(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            // For reference this implementation was ultimately found through this post:
            // http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-metro 

            WIC.BitmapDecoder decoder;
            using (WIC.BitmapSource bmpSource = LoadBitmap_DX(stream, out decoder))
            using (decoder)
            {
                width = bmpSource.Size.Width;
                height = bmpSource.Size.Height;
                ValidateBounds(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy, width, height);

                // TODO: use texture.SetData(...)
                D3D11.Texture2DDescription texture2DDesc;
                texture2DDesc.Width = width;
                texture2DDesc.Height = height;
                texture2DDesc.ArraySize = 1;
                texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
                texture2DDesc.Usage = D3D11.ResourceUsage.Default;
                texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                texture2DDesc.Format = DXGI.Format.R8G8B8A8_UNorm;
                texture2DDesc.MipLevels = 1;
                texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
                texture2DDesc.SampleDescription.Count = 1;
                texture2DDesc.SampleDescription.Quality = 0;

                using (DX.DataStream dataStream = new DX.DataStream(bmpSource.Size.Height * bmpSource.Size.Width * 4, true, true))
                {
                    bmpSource.CopyPixels(bmpSource.Size.Width * 4, dataStream);
                    DX.DataRectangle rect = new DX.DataRectangle(dataStream.DataPointer, bmpSource.Size.Width * 4);
                    _texture = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc, rect);
                    _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _texture);
                }
            }
        }

        private static unsafe void ValidateBounds(GraphicsDeviceStrategy deviceStrategy, int width, int height)
        {
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.Reach && (width > 2048 || height > 2048))
                throw new NotSupportedException("Reach profile supports a maximum Texture2D size of 2048");
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.HiDef && (width > 4096 || height > 4096))
                throw new NotSupportedException("HiDef profile supports a maximum Texture2D size of 4096");
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.FL10_0 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_0 profile supports a maximum Texture2D size of 8192");
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.FL10_1 && (width > 8192 || height > 8192))
                throw new NotSupportedException("FL10_1 profile supports a maximum Texture2D size of 8192");
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.FL11_0 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_0 profile supports a maximum Texture2D size of 16384");
            if (deviceStrategy.GraphicsProfile == GraphicsProfile.FL11_1 && (width > 16384 || height > 16384))
                throw new NotSupportedException("FL11_1 profile supports a maximum Texture2D size of 16384");
        }

        public void SaveAsPng(Stream stream, int width, int height)
        {
            PngWriter pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
#if UAP || WINUI
            Task result = SaveAsImageAsync_UAP(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream, width, height);
            result.Wait();
#else
            throw new NotImplementedException();
#endif
        }

#if UAP || WINUI
        private async Task SaveAsImageAsync_UAP(Guid encoderId, Stream stream, int width, int height)
        {
            byte[] pixelData = new byte[Width * Height * Format.GetSize()];
            GetData(0,0, Bounds, pixelData, 0, pixelData.Length);

            // TODO: We need to convert from Format to R8G8B8A8!

            // Create a temporary memory stream for writing the png.
            InMemoryRandomAccessStream memstream = new InMemoryRandomAccessStream();

            // Write the png.
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, memstream);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixelData);
            await encoder.FlushAsync();

            // Copy the memory stream into the real output stream.
            memstream.Seek(0);
            memstream.AsStreamForRead().CopyTo(stream);
        }
#endif

        //Converts Pixel Data from BGRA to RGBA
        private static void ConvertToRGBA(int pixelHeight, int pixelWidth, byte[] pixels)
        {
            int offset = 0;

            for (int row = 0; row < (uint)pixelHeight; row++)
            {
                int rowxPixelWidth = row * pixelWidth * 4;
                for (int col = 0; col < (uint)pixelWidth; col++)
                {
                    offset = rowxPixelWidth + (col * 4);

                    byte B = pixels[offset];
                    byte R = pixels[offset + 2];

                    pixels[offset] = R;
                    pixels[offset + 2] = B;
                }
            }
        }

        static unsafe D3D11.Texture2D CreateTex2DFromBitmap(WIC.BitmapSource bsource, GraphicsDevice device)
        {
            D3D11.Texture2DDescription texture2DDesc;
            texture2DDesc.Width = bsource.Size.Width;
            texture2DDesc.Height = bsource.Size.Height;
            texture2DDesc.ArraySize = 1;
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.Format = DXGI.Format.R8G8B8A8_UNorm;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            texture2DDesc.SampleDescription.Count = 1;
            texture2DDesc.SampleDescription.Quality = 0;

            using (DX.DataStream s = new DX.DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
            {
                bsource.CopyPixels(bsource.Size.Width * 4, s);

                // XNA blacks out any pixels with an alpha of zero.
                /* TNC: Disable XNA behavior.
                // TNC: We either preserve the original data or premultiply the entire image.
                byte* data = (byte*)s.DataPointer;
                for (int i = 0; i < s.Length; i+=4)
                {
                    if (data[i + 3] == 0)
                    {
                        data[i + 0] = 0;
                        data[i + 1] = 0;
                        data[i + 2] = 0;
                    }
                }
                */

                DX.DataRectangle rect = new DX.DataRectangle(s.DataPointer, bsource.Size.Width * 4);

                return new D3D11.Texture2D(((IPlatformGraphicsDevice)device).Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc, rect);
            }
        }

        static WIC.ImagingFactory imgfactory;
        private static WIC.BitmapSource LoadBitmap(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory == null)
            {
                imgfactory = new WIC.ImagingFactory();
            }

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory,
                stream,
                WIC.DecodeOptions.CacheOnDemand
                );

            WIC.FormatConverter fconv = new WIC.FormatConverter(imgfactory);

            using (WIC.BitmapFrameDecode frame = decoder.GetFrame(0))
            {
                fconv.Initialize(
                    frame,
                    WIC.PixelFormat.Format32bppRGBA,
                    WIC.BitmapDitherType.None,
                    null,
                    0.0,
                    WIC.BitmapPaletteType.Custom);
            }
            return fconv;
        }    
    
    }
}
