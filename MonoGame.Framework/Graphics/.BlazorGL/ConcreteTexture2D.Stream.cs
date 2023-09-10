// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;
using StbImageSharp;
using StbImageWriteSharp;


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

            Color[] data = Texture2D.GetColorData(this);
            fixed (Color* pData = data)
            {
                ImageWriter writer = new ImageWriter();
                writer.WritePng(pData, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        public unsafe void SaveAsJpeg(Stream stream, int width, int height)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "'stream' cannot be null");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");

            Color[] data = Texture2D.GetColorData(this);
            fixed (Color* pData = data)
            {
                ImageWriter writer = new ImageWriter();
                writer.WriteJpg(pData, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
            }
        }

    }
}
