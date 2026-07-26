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
using Microsoft.Xna.Platform.Graphics.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTexture3D : ConcreteTexture, ITexture3DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        internal readonly bool _mipMap;


        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format,
                                   bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this._mipMap = mipMap;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, width, height, depth))
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this._mipMap = mipMap;

            this.PlatformConstructTexture3D(contextStrategy, width, height, depth, mipMap, format);
        }


        #region ITexture3DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Depth { get { return _depth; } }

        public unsafe void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                      T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            int elementSizeInByte = sizeof(T);
            fixed (T* pData = &data[0])
            {
                IntPtr dataPtr = (IntPtr)pData;
                dataPtr = dataPtr + startIndex * elementSizeInByte;

                int rowPitch = this.Format.GetPitch(width);
                int rowCount = this.Format.IsCompressedFormat() ? ((height + 3) / 4) : height;
                int slicePitch = rowPitch * rowCount; // For 3D texture: Size of 2D image.
                DX.DataBox dataBox = new DX.DataBox(dataPtr, rowPitch, slicePitch);

                int subresourceIndex = level;

                D3D11.ResourceRegion region = new D3D11.ResourceRegion(left, top, front, right, bottom, back);

                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(dataBox, this.GetTexture(), subresourceIndex, region);
                }
            }
        }

        public unsafe void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                      T[] data, int startIndex, int elementCount)
             where T : struct
        {
            // Create a temp staging resource for copying the data.
            //
            int min = this.Format.IsCompressedFormat() ? 4 : 1;
            int levelWidth = Math.Max(this.Width >> level, min);
            int levelHeight = Math.Max(this.Height >> level, min);
            int levelDepth = Math.Max(this.Depth >> level, min);

            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = levelWidth;
            texture3DDesc.Height = levelHeight;
            texture3DDesc.Depth = levelDepth;
            texture3DDesc.MipLevels = 1;
            texture3DDesc.Format = this.Format.ToDXFormat();
            texture3DDesc.BindFlags = D3D11.BindFlags.None;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            texture3DDesc.Usage = D3D11.ResourceUsage.Staging;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            using (D3D11.Texture3D stagingTexture = new D3D11.Texture3D(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc))
            {
                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    // Copy the data from the GPU to the staging texture.
                    d3dContext.CopySubresourceRegion(this.GetTexture(), level, new D3D11.ResourceRegion(left, top, front, right, bottom, back), stagingTexture, 0);

                    // Copy the data to the array.
                    DX.DataStream stream = null;
                    DX.DataBox dataBox = d3dContext.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);
                    try
                    {
                        int elementSize = this.Format.GetSize();
                        int elementsInRow = right - left;
                        int rowsInSlice = bottom - top;
                        int slices = back - front;
                        if (this.Format.IsCompressedFormat())
                        {
                            // for 4x4 block compression formats an element is one block, so elementsInRow
                            // and number of rows are 1/4 of number of pixels in width and height
                            elementsInRow /= 4;
                            rowsInSlice /= 4;
                        }
                        int rowSize = elementSize * elementsInRow;
                        int sliceSize = rowSize * rowsInSlice;
                        if ((rowSize == dataBox.RowPitch) && (sliceSize == dataBox.SlicePitch))
                            stream.ReadRange(data, startIndex, elementCount);
                        else if (left == 0 && top == 0 && front == 0 &&
                                 right == levelWidth && bottom == levelHeight && back == levelDepth &&
                                 startIndex == 0 && elementCount == data.Length)
                        {
                            // Optimized PlatformGetData() that reads multiple elements in a row when texture has rowPitch
                            int elementSize2 = sizeof(T);
                            if (elementSize2 == 1) // byte[]
                                elementsInRow = elementsInRow * elementSize;

                            int currentIndex = 0;
                            for (int slice = front; slice < back; slice++)
                            {
                                for (int row = 0; row < rowsInSlice; row++)
                                {
                                    stream.ReadRange(data, currentIndex, elementsInRow);
                                    stream.Seek(dataBox.RowPitch - rowSize, SeekOrigin.Current);
                                    currentIndex += elementsInRow;
                                }
                                stream.Seek(dataBox.SlicePitch - (dataBox.RowPitch * rowsInSlice), SeekOrigin.Current);
                            }
                        }
                        else
                        {
                            // Some drivers may add pitch to rows or slices.
                            // We need to copy each row separately and skip trailing zeros.
                            stream.Seek(0, SeekOrigin.Begin);

                            int elementSizeInByte = sizeof(T);
                            for (int slice = 0; slice < slices; slice++)
                            {
                                int dataSliceOffset = slice * sliceSize / elementSizeInByte;
                                for (int row = 0; row < rowsInSlice; row++)
                                {
                                    int i;
                                    int maxElements = (row + 1) * rowSize / elementSizeInByte;
                                    for (i = row * rowSize / elementSizeInByte; i < maxElements; i++)
                                        data[i + dataSliceOffset + startIndex] = stream.Read<T>();

                                    if ((i + dataSliceOffset) >= elementCount)
                                        break;

                                    stream.Seek(dataBox.RowPitch - rowSize, SeekOrigin.Current);
                                }

                                if ((dataSliceOffset + (rowsInSlice * elementsInRow)) >= elementCount)
                                    break;

                                stream.Seek(dataBox.SlicePitch - (dataBox.RowPitch * rowsInSlice), SeekOrigin.Current);
                            }
                        }
                    }
                    finally
                    {
                        DX.Utilities.Dispose(ref stream);

                        d3dContext.UnmapSubresource(stagingTexture, 0);
                    }
                }
            }
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
            checkedRight = checkedLeft + roundedWidth;
            checkedBottom = checkedTop + roundedHeight;
            checkedFront = front;
            checkedBack = back;
            return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight) * depth;
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
            checkedRight = checkedLeft + roundedWidth;
            checkedBottom = checkedTop + roundedHeight;
            checkedFront = front;
            checkedBack = back;
            return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight) * depth;
        }
        #endregion ITexture3DStrategy

        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = this.Format.ToDXFormat();
            texture3DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture3D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

    }
}
