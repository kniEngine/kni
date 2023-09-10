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
using MonoGame.Framework.Utilities;
using MonoGame.Utilities.Png;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using WIC = SharpDX.WIC;

#if WINDOWS_UAP
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif


namespace Microsoft.Xna.Platform.Graphics
{
    internal partial class ConcreteTexture2D
    {

        public void SaveAsPng(Stream stream, int width, int height)
        {
            PngWriter pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
#if WINDOWS_UAP
            Task result = SaveAsImageAsync_UAP(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream, width, height);
            result.Wait();
#else
            throw new NotImplementedException();
#endif
        }

#if WINDOWS_UAP
        private async Task SaveAsImageAsync_UAP(Guid encoderId, Stream stream, int width, int height)
        {
            byte[] pixelData = new byte[Width * Height * GraphicsExtensions.GetSize(Format)];
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
                var data = (byte*)s.DataPointer;
                for (var i = 0; i < s.Length; i+=4)
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

                return new D3D11.Texture2D(device.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc, rect);
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

            var fconv = new WIC.FormatConverter(imgfactory);

            using (var frame = decoder.GetFrame(0))
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
