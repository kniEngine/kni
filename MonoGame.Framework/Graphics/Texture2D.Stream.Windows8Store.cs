// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.Utilities.Png;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
#if !WP8
using SharpDX.WIC;
using StbImageSharp;
#endif
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

#if (W81 || WP81)
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

#if WP8
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
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

#if (W81 || WP81)
            if (stream.CanSeek)
                return PlatformFromStream_W81(graphicsDevice, stream);
#endif
#if WP8
            return PlatformFromStream_WP8(graphicsDevice, stream);
#else
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
#endif
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if (W81 || WP81)
            SaveAsImage_W81(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream, width, height);
#elif WP8
            var pixelData = new byte[Width * Height * GraphicsExtensions.GetSize(Format)];
            GetData(pixelData);

            //We Must convert from BGRA to RGBA
            ConvertToRGBA(Height, Width, pixelData);

            var waitEvent = new ManualResetEventSlim(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var bitmap = new WriteableBitmap(Width, Height);
                System.Buffer.BlockCopy(pixelData, 0, bitmap.Pixels, 0, pixelData.Length);
                bitmap.SaveJpeg(stream, width, height, 0, 100);
                waitEvent.Set();
            });

            waitEvent.Wait();
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
#if (WP81 || W81)
            SaveAsImage_W81(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream, width, height);
            //SaveAsImage(Windows.Graphics.BitmapEncoder.PngEncoderId, stream, width, height);
            return;
#endif
            var pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
        }
        
#if !WP8
        static unsafe SharpDX.Direct3D11.Texture2D CreateTex2DFromBitmap(BitmapSource bsource, GraphicsDevice device)
        {
            Texture2DDescription desc;
            desc.Width = bsource.Size.Width;
            desc.Height = bsource.Size.Height;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            using (DataStream s = new DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
            {
                bsource.CopyPixels(bsource.Size.Width * 4, s);

                // XNA blacks out any pixels with an alpha of zero.
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

                DataRectangle rect = new DataRectangle(s.DataPointer, bsource.Size.Width * 4);

                return new SharpDX.Direct3D11.Texture2D(device._d3dDevice, desc, rect);
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
#endif
    
    }
}

