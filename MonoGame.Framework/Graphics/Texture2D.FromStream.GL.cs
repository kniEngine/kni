// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2019 Kastellanos Nikos

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.Utilities.Png;
using StbImageSharp;

#if IOS || TVOS
using UIKit;
using CoreGraphics;
using Foundation;
using System.Drawing;
#endif

#if OPENGL
using MonoGame.OpenGL;
using GLPixelFormat = MonoGame.OpenGL.PixelFormat;
using PixelFormat = MonoGame.OpenGL.PixelFormat;

#if ANDROID
using Android.Graphics;
#endif
#endif // OPENGL

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

#if IOS || TVOS
             return PlatformFromStream_IOS(graphicsDevice, stream);
#elif ANDROID
            return PlatformFromStream_ANDROID(graphicsDevice, stream);
#elif DESKTOPGL && (NET40 || NET40_OR_GREATER)
            return PlatformFromStream_DESKTOPGL(graphicsDevice, stream);
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
            var result = ImageResult.FromMemory(bytes, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, result.Width, result.Height);
            texture.SetData(result.Data);

            return texture;
#endif
        }


        #region IOS || TVOS
#if IOS || TVOS
        private unsafe static Texture2D PlatformFromStream_IOS(GraphicsDevice graphicsDevice, Stream stream)
        {
			using (var uiImage = UIImage.LoadFromData(NSData.FromStream(stream)))
            {
				var cgImage = uiImage.CGImage;
                return PlatformFromImage_IOS(graphicsDevice, cgImage);
			}
        }

        private static Texture2D PlatformFromImage_IOS(GraphicsDevice graphicsDevice, CGImage cgImage)
        {
            int width = (int)cgImage.Width;
            int height = (int)cgImage.Height;

            var data = new byte[width * height * 4];

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace,               
                CGBitmapFlags.Last // CGBitmapFlags.PremultipliedLast
                );
                
            bitmapContext.DrawImage(new System.Drawing.RectangleF(0, 0, width, height), cgImage);
            bitmapContext.Dispose();
            colorSpace.Dispose();

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, (int)width, (int)height, false, SurfaceFormat.Color);
            texture.SetData(data);

            return texture;
        }
#endif
        #endregion IOS || TVOS


        #region ANDROID
#if ANDROID
        private unsafe static Texture2D PlatformFromStream_ANDROID(GraphicsDevice graphicsDevice, Stream stream)
        {
            using (Bitmap image = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,
                InPremultiplied = false,
            }))
            {
                return PlatformFromBitmap_ANDROID(graphicsDevice, image);
            }
        }

        private static Texture2D PlatformFromBitmap_ANDROID(GraphicsDevice graphicsDevice, Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                using (Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    Canvas canvas = new Canvas(imagePadded);
                    canvas.DrawARGB(0, 0, 0, 0);
                    canvas.DrawBitmap(image, 0, 0, null);
                    imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
                    imagePadded.Recycle();
                }
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }
            image.Recycle();

            // Convert from ARGB to ABGR
            ConvertToABGR_ANDROID(height, width, pixels);

            Texture2D texture = null;
            texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData<int>(pixels);

            return texture;
        }
        
        //Converts Pixel Data from ARGB to ABGR
        private static void ConvertToABGR_ANDROID(int pixelHeight, int pixelWidth, int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for (int i = 0; i < pixelCount; i++)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }
#endif
        #endregion ANDROID


        #region DESKTOPGL 
#if DESKTOPGL && (NET40 || NET40_OR_GREATER)
        private unsafe static Texture2D PlatformFromStream_DESKTOPGL(GraphicsDevice graphicsDevice, Stream stream)
        {
            System.Drawing.Bitmap image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
            try
            {
                if ((image.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0)
                    image = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                                
                System.Drawing.Imaging.BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (bitmapData.Stride != image.Width * 4)
                    throw new NotImplementedException();                
                var data = new byte[image.Width * image.Height * 4];
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);
                
                // Convert from ARGB to ABGR
                ConvertToABGR_DESKTOPGL(image.Height, image.Width, data);

                 Texture2D texture = null;
                texture = new Texture2D(graphicsDevice, image.Width, image.Height);
                texture.SetData(data);
                 return texture;
            }
            finally
            {
                image.Dispose();
            }
        }
        
        private unsafe static void ConvertToABGR_DESKTOPGL(int pixelHeight, int pixelWidth, byte[] data)
        {            
            int pixelCount = pixelWidth * pixelHeight;
            fixed (byte* pdata = data)
            {
                for (int i = 0; i < pixelCount; i++)
                {
                    var t = pdata[i * 4 + 0];
                    pdata[i * 4 + 0] = pdata[i * 4 + 2];
                    pdata[i * 4 + 2] = t;
                }
            }
        }
#endif
        #endregion DESKTOPGL
    }
}
