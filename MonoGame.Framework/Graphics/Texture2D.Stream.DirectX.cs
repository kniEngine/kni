// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Utilities.Png;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

using SharpDX.WIC;
using StbImageSharp;

#if WINDOWS_UAP
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        
        private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            
#if WINDOWS || WINDOWS_UAP
            if (stream.CanSeek)
                return PlatformFromStream_DX(graphicsDevice, stream);
#endif
            byte[] bytes;

            // Copy it's data to memory
            // As some platforms dont provide full stream functionality and thus streams can't be read as it is
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            // The data returned is always four channel BGRA
            var result = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, result.Width, result.Height);
            texture.SetData(result.Data);

            return texture;
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if WINDOWS_UAP
            SaveAsImage(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream, width, height);
#else
            throw new NotImplementedException();
#endif
        }

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

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            var pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
        }

#if WINDOWS_UAP
        private void SaveAsImage(Guid encoderId, Stream stream, int width, int height)
        {
            var pixelData = new byte[Width * Height * GraphicsExtensions.GetSize(Format)];
            GetData(pixelData);

            // TODO: We need to convert from Format to R8G8B8A8!

            // TODO: We should implement async SaveAsPng() for WinRT.
            Task.Run(async () =>
            {
                // Create a temporary memory stream for writing the png.
                var memstream = new InMemoryRandomAccessStream();

                // Write the png.
                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(encoderId, memstream);
                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixelData);
                await encoder.FlushAsync();

                // Copy the memory stream into the real output stream.
                memstream.Seek(0);
                memstream.AsStreamForRead().CopyTo(stream);

            }).Wait();
        }
#endif

        static unsafe D3D11.Texture2D CreateTex2DFromBitmap(BitmapSource bsource, GraphicsDevice device)
        {
            D3D11.Texture2DDescription desc;
            desc.Width = bsource.Size.Width;
            desc.Height = bsource.Size.Height;
            desc.ArraySize = 1;
            desc.BindFlags = D3D11.BindFlags.ShaderResource;
            desc.Usage = D3D11.ResourceUsage.Default;
            desc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            desc.Format = DXGI.Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = D3D11.ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            using (DataStream s = new DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
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

                DataRectangle rect = new DataRectangle(s.DataPointer, bsource.Size.Width * 4);

                return new D3D11.Texture2D(device.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, desc, rect);
            }
        }

        static ImagingFactory imgfactory;
        private static BitmapSource LoadBitmap(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory == null)
            {
                imgfactory = new ImagingFactory();
            }

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory,
                stream,
                DecodeOptions.CacheOnDemand
                );

            var fconv = new FormatConverter(imgfactory);

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
    
    }
}

