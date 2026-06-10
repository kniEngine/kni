// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;


        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format,
                                   bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this.PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            throw new NotImplementedException();
        }

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
            throw new NotImplementedException();
        }

        public int GetCompressedDataByteSize(int fSize, int left, int top, int right, int bottom, int front, int back,
                                             int textureBoundsWidth, int textureBoundsHeight, int textureBoundsDepth,
                                             out int checkedLeft, out int checkedTop, out int checkedRight, out int checkedBottom,
                                             out int checkedFront, out int checkedBack)
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;
            Format.GetBlockSize(out int blockWidth, out int blockHeight);
            int blockWidthMinusOne = blockWidth - 1;
            int blockHeightMinusOne = blockHeight - 1;
            // round x and y down to next multiple of block size; width and height up to next multiple of block size
            int roundedWidth = (width + blockWidthMinusOne) & ~blockWidthMinusOne;
            int roundedHeight = (height + blockHeightMinusOne) & ~blockHeightMinusOne;
            checkedLeft = left & ~blockWidthMinusOne;
            checkedTop = top & ~blockHeightMinusOne;
            // The last two mip levels require the width and height to be passed
            // as 2x2 and 1x1, but there needs to be enough data passed to occupy a full block.
            checkedRight = checkedLeft + ((width < blockWidth && textureBoundsWidth < blockWidth) ? textureBoundsWidth : roundedWidth);
            checkedBottom = checkedTop + ((height < blockHeight && textureBoundsHeight < blockHeight) ? textureBoundsHeight : roundedHeight);
            checkedFront = front;
            checkedBack = back;
            if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
            {
                return (Math.Max(checkedRight - checkedLeft, 16) * Math.Max(checkedBottom - checkedTop, 8) * 2 + 7) / 8 * depth;
            }
            else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
            {
                return (Math.Max(checkedRight - checkedLeft, 8) * Math.Max(checkedBottom - checkedTop, 8) * 4 + 7) / 8 * depth;
            }
            else
            {
                return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight) * depth;
            }
        }
        #endregion ITexture3DStrategy

        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            base.Dispose(disposing);
        }

    }
}
