// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2019 Kastellanos Nikos

using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        

        private unsafe static Texture2D PlatformFromStream_WP8(GraphicsDevice graphicsDevice, Stream stream)
        {
            WriteableBitmap bitmap = null;
            var waitEvent = new ManualResetEventSlim(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(stream);
                    bitmap = new WriteableBitmap(bitmapImage);
                    waitEvent.Set();
            });
            waitEvent.Wait();

            // Convert from ARGB to ABGR 
            ConvertToABGR_WP8(bitmap.PixelHeight, bitmap.PixelWidth, bitmap.Pixels);

            Texture2D texture = new Texture2D(graphicsDevice, bitmap.PixelWidth, bitmap.PixelHeight);
            texture.SetData<int>(bitmap.Pixels);
            return texture;
        }
        
        //Converts Pixel Data from ARGB to ABGR
        private static void ConvertToABGR_WP8(int pixelHeight, int pixelWidth, int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for (int i = 0; i < pixelCount; ++i)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }

    }
}

