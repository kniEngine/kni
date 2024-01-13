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
using Microsoft.Xna.Platform.Graphics.OpenGL;


#if DESKTOPGL
using StbImageWriteSharp;
#endif

#if ANDROID
using Android.Graphics;
#endif

#if IOS || TVOS
using UIKit;
using CoreGraphics;
using Foundation;
using MonoGame.Utilities.Png;
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

#if DESKTOPGL && (NET40 || NET40_OR_GREATER)
            PlatformFromStream_DESKTOPGL(contextStrategy, stream, out this._width, out this._height);
#elif ANDROID
            PlatformFromStream_ANDROID(contextStrategy, stream, out this._width, out this._height);
#elif IOS || TVOS
            PlatformFromStream_IOS(contextStrategy, stream, out this._width, out this._height);
#else
            if (stream.CanSeek)
            {
                PlatformFromStream_ImageSharp(contextStrategy, stream, out this._width, out this._height);
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

#endif
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

        #region DESKTOPGL 
#if DESKTOPGL && (NET40 || NET40_OR_GREATER)
        private unsafe void PlatformFromStream_DESKTOPGL(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            System.Drawing.Bitmap image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
            try
            {
                if ((image.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0)
                    image = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                width = image.Width;
                height = image.Height;
                ValidateBounds(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy, width, height);

                byte[] data = new byte[width * height * 4];

                System.Drawing.Imaging.BitmapData bitmapData = null;
                try
                {
                    bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    if (bitmapData.Stride != image.Width * 4)
                        throw new NotImplementedException();

                    Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                }
                finally
                {
                    image.UnlockBits(bitmapData);
                }
                
                // Convert from ARGB to ABGR
                ConvertToABGR_DESKTOPGL(width, height, data);

                this.PlatformConstructTexture2D(contextStrategy, width, height, false, SurfaceFormat.Color, false);
                this.SetData<byte>(0, data, 0, data.Length);
            }
            finally
            {
                image.Dispose();
            }
        }
        
        private unsafe static void ConvertToABGR_DESKTOPGL(int pixelWidth, int pixelHeight, byte[] data)
        {            
            int pixelCount = pixelWidth * pixelHeight;
            fixed (byte* pData = data)
            {
                for (int i = 0; i < pixelCount; i++)
                {
                    byte t = pData[i * 4 + 0];
                    pData[i * 4 + 0] = pData[i * 4 + 2];
                    pData[i * 4 + 2] = t;
                }
            }
        }
#endif
        #endregion DESKTOPGL

        #region ANDROID
#if ANDROID
        private unsafe void PlatformFromStream_ANDROID(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            using (Android.Graphics.Bitmap image = Android.Graphics.BitmapFactory.DecodeStream(stream, null,
                new Android.Graphics.BitmapFactory.Options
                {
                    InScaled = false,
                    InDither = false,
                    InJustDecodeBounds = false,
                    InPurgeable = true,
                    InInputShareable = true,
                    InPremultiplied = false,
                }))
            {
                width = image.Width;
                height = image.Height;
                ValidateBounds(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy, width, height);

                int[] data = new int[width * height];
                if ((width != image.Width) || (height != image.Height))
                {
                    using (Android.Graphics.Bitmap imagePadded = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888))
                    {
                        Android.Graphics.Canvas canvas = new Android.Graphics.Canvas(imagePadded);
                        canvas.DrawARGB(0, 0, 0, 0);
                        canvas.DrawBitmap(image, 0, 0, null);
                        imagePadded.GetPixels(data, 0, width, 0, 0, width, height);
                        imagePadded.Recycle();
                    }
                }
                else
                {
                    image.GetPixels(data, 0, width, 0, 0, width, height);
                }
                image.Recycle();

                // Convert from ARGB to ABGR
                ConvertToABGR_ANDROID(width, height, data);

                this.PlatformConstructTexture2D(contextStrategy, width, height, false, SurfaceFormat.Color, false);
                this.SetData<int>(0, data, 0, data.Length);
            }
        }

        //Converts Pixel Data from ARGB to ABGR
        private void ConvertToABGR_ANDROID(int pixelWidth, int pixelHeight, int[] pixels)
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

        #region IOS || TVOS
#if IOS || TVOS
        private unsafe void PlatformFromStream_IOS(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            using (UIImage uiImage = UIImage.LoadFromData(NSData.FromStream(stream)))
            {
                CGImage cgImage = uiImage.CGImage;
                width = (int)cgImage.Width;
                height = (int)cgImage.Height;
                ValidateBounds(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy, width, height);
                
                byte[] data = new byte[width * height * 4];

                CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
                CGBitmapContext bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace,
                    CGBitmapFlags.Last // CGBitmapFlags.PremultipliedLast
                    );

                bitmapContext.DrawImage(new System.Drawing.RectangleF(0, 0, width, height), cgImage);
                bitmapContext.Dispose();
                colorSpace.Dispose();

                this.PlatformConstructTexture2D(contextStrategy, width, height, false, SurfaceFormat.Color, false);
                this.SetData<byte>(0, data, 0, data.Length);
            }
        }
#endif
        #endregion IOS || TVOS


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

        public unsafe void SaveAsPng(Stream stream, int width, int height)
        {
	        if (stream == null)
		          throw new ArgumentNullException("stream", "'stream' cannot be null");
	        if (width <= 0)
		          throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
	        if (height <= 0)
		          throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");

#if DESKTOPGL
            Color[] data = Texture2D.GetColorData(this);
            fixed (Color* pData = data)
            {
                ImageWriter writer = new ImageWriter();
                writer.WritePng(pData, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
#elif ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Png);
#else
            PngWriter pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
#endif
        }

        public unsafe void SaveAsJpeg(Stream stream, int width, int height)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "'stream' cannot be null");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");

#if DESKTOPGL
            Color[] data = Texture2D.GetColorData(this);
            fixed (Color* pData = data)
            {
                ImageWriter writer = new ImageWriter();
                writer.WriteJpg(pData, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
            }
#elif ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Jpeg);
#else
            throw new NotImplementedException();
#endif
        }

#if ANDROID
        private void SaveAsImage(Stream stream, int width, int height, Bitmap.CompressFormat format)
        {
            int[] data = new int[width * height];
            GetData<int>(0, 0, Bounds, data, 0, data.Length);
            
            // internal structure is BGR while bitmap expects RGB
            for (int i = 0; i < data.Length; i++)
            {
                uint pixel = (uint)data[i];
                data[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
            
            using (Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
            {
                bitmap.SetPixels(data, 0, width, 0, 0, width, height);
                bitmap.Compress(format, 100, stream);
                bitmap.Recycle();
            }
        }
#endif

    }

}
