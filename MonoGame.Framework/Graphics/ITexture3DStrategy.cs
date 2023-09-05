// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface ITexture3DStrategy : ITextureStrategy
    {

        int Width { get; }
        int Height { get; }
        int Depth { get; }

        void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct;
        void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct;
    }
}
