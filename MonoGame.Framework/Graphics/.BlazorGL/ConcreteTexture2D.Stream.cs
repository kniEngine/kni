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
        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, Stream stream)
            : base(contextStrategy, SurfaceFormat.Color, 1)
        {
            this._arraySize = 1;

            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
                stream.Seek(0, SeekOrigin.Begin);

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
        }

        private unsafe void PlatformFromStream_ImageSharp(GraphicsContextStrategy contextStrategy, Stream stream, out int width, out int height)
        {
            StbImageSharp.ImageResult result = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            width = result.Width;
            height = result.Height;
            ValidateBounds(contextStrategy.Context.DeviceStrategy, width, height);

            this.PlatformConstructTexture2D(contextStrategy, width, height, false, SurfaceFormat.Color, false);
            this.SetData<byte>(0, result.Data, 0, result.Data.Length);
        }

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
