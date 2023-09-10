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
using MonoGame.OpenGL;


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
