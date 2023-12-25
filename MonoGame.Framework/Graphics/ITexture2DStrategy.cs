// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface ITexture2DStrategy : ITextureStrategy
    {

        int Width { get; }
        int Height { get; }
        int ArraySize { get; }

        Rectangle Bounds { get; }

        IntPtr GetSharedHandle();
        void SetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct;
        void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount) where T : struct;
        void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount) where T : struct;
        int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect);

        void SaveAsJpeg(Stream stream, int width, int height);
        void SaveAsPng(Stream stream, int width, int height);
    }
}
