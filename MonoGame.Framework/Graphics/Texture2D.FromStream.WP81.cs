// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2019 Kastellanos Nikos

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.Utilities.Png;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        static ImagingFactory imgfactory_W81;

        private static BitmapSource LoadBitmap_W81(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory_W81 == null)
                imgfactory_W81 = new ImagingFactory();

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory_W81,
                stream,
                DecodeOptions.CacheOnDemand
                );

            var fconv = new FormatConverter(imgfactory_W81);

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

        private unsafe static Texture2D PlatformFromStream_W81(GraphicsDevice graphicsDevice, Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("stream must support seek operations");
            
            // For reference this implementation was ultimately found through this post:
            // http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-metro 

            SharpDX.WIC.BitmapDecoder decoder;
            using (var bmpSource = LoadBitmap_W81(stream, out decoder))
            using (decoder)
            {
                Texture2D texture = new Texture2D(graphicsDevice, bmpSource.Size.Width, bmpSource.Size.Height);

                // TODO: use texture.SetData(...)
                Texture2DDescription desc;
                desc.Width = bmpSource.Size.Width;
                desc.Height = bmpSource.Size.Height;
                desc.ArraySize = 1;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.Usage = ResourceUsage.Default;
                desc.CpuAccessFlags = CpuAccessFlags.None;
                desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                desc.MipLevels = 1;
                desc.OptionFlags = ResourceOptionFlags.None;
                desc.SampleDescription.Count = 1;
                desc.SampleDescription.Quality = 0;

                SharpDX.Direct3D11.Texture2D textureResource;
                using (DataStream s = new DataStream(bmpSource.Size.Height * bmpSource.Size.Width * 4, true, true))
                {
                    bmpSource.CopyPixels(bmpSource.Size.Width * 4, s);
                    DataRectangle rect = new DataRectangle(s.DataPointer, bmpSource.Size.Width * 4);
                    textureResource = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, desc, rect);
                }
                texture.SetTextureInternal_W81(textureResource);

                return texture;
            }
        }

        private void SaveAsImage_W81(Guid encoderId, Stream stream, int width, int height)
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

    }
}

