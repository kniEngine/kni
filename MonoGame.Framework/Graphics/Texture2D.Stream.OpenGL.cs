// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
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
using StbImageWriteSharp;

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

#if DESKTOPGL
        internal enum ImageWriterFormat
        {
            Jpg,
            Png
        }
#endif


        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if DESKTOPGL
            SaveAsImage(stream, width, height, ImageWriterFormat.Jpg);
#elif ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Jpeg);
#else
            throw new NotImplementedException();
#endif
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
#if DESKTOPGL
            SaveAsImage(stream, width, height, ImageWriterFormat.Png);
#elif ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Png);
#else
            var pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
#endif
        }


#if DESKTOPGL
        internal unsafe void SaveAsImage(Stream stream, int width, int height, ImageWriterFormat format)
        {
	        if (stream == null)
		          throw new ArgumentNullException("stream", "'stream' cannot be null");
	        if (width <= 0)
		          throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
	        if (height <= 0)
		          throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");
		          
	        Color[] data = null;
	        try
	        {
                data = GetColorData();

                // Write
                fixed (Color* ptr = &data[0])
                {
                    var writer = new ImageWriter();
                    switch (format)
                    {
                        case ImageWriterFormat.Jpg:
                            writer.WriteJpg(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
                            break;
                        case ImageWriterFormat.Png:
                            writer.WritePng(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                            break;
                    }
                }
            }
            finally
	        {
		        if (data != null)
		        {
			        data = null;
		        }
	        }
        }
#elif ANDROID
        private void SaveAsImage(Stream stream, int width, int height, Bitmap.CompressFormat format)
        {
            int[] data = new int[width * height];
            GetData(data);
            
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
