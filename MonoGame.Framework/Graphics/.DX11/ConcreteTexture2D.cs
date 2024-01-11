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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal partial class ConcreteTexture2D : ConcreteTexture, ITexture2DStrategy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _arraySize;

        internal readonly bool _mipMap;
        internal readonly bool _shared;


        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared,
                                   bool isRenderTarget)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;

            this._mipMap = mipMap;
            this._shared = shared;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height))
        {
            this._width  = width;
            this._height = height;
            this._arraySize = arraySize;

            this._mipMap = mipMap;
            this._shared = shared;

            this.PlatformConstructTexture2D(contextStrategy, width, height, mipMap, format, shared);
        }


        #region ITexture2DStrategy
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int ArraySize { get { return _arraySize; } }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, this._width, this._height); }
        }

        public IntPtr GetSharedHandle()
        {
            using (DXGI.Resource resource = GetTexture().QueryInterface<DXGI.Resource>())
            {
                return resource.SharedHandle;
            }
        }

        public void SetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int w, h;
            Texture.GetSizeForLevel(Width, Height, level, out w, out h);

            // For DXT compressed formats the width and height must be
            // a multiple of 4 for the complete mip level to be set.
            if (this.Format.IsCompressedFormat())
            {
                w = (w + 3) & ~3;
                h = (h + 3) & ~3;
            }

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                D3D11.ResourceRegion region = new D3D11.ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = h;
                region.Left = 0;
                region.Right = w;

                // TODO: We need to deal with threaded contexts here!
                int arraySlice = 0;
                int subresourceIndex = arraySlice * this.LevelCount + level;
                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(this.GetTexture(), subresourceIndex, region, dataPtr, Texture.GetPitch(this.Format, w), 0);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                int startBytes = startIndex * elementSizeInByte;
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                D3D11.ResourceRegion region = new D3D11.ResourceRegion();
                region.Top = checkedRect.Top;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = checkedRect.Bottom;
                region.Left = checkedRect.Left;
                region.Right = checkedRect.Right;


                // TODO: We need to deal with threaded contexts here!
                int subresourceIndex = arraySlice * this.LevelCount + level;
                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(this.GetTexture(), subresourceIndex, region, dataPtr, Texture.GetPitch(this.Format, checkedRect.Width), 0);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            // Create a temp staging resource for copying the data.
            //
            int min = this.Format.IsCompressedFormat() ? 4 : 1;
            int levelWidth = Math.Max(this.Width >> level, min);
            int levelHeight = Math.Max(this.Height >> level, min);

            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = levelWidth;
            texture2DDesc.Height = levelHeight;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.ArraySize = 1;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.None;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Staging;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            D3D11.Texture2D stagingTexture = new D3D11.Texture2D(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);

            lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                int subresourceIndex = arraySlice * this.LevelCount + level;

                // Copy the data from the GPU to the staging texture.
                int elementsInRow = checkedRect.Width;
                int rows = checkedRect.Height;
                D3D11.ResourceRegion region = new D3D11.ResourceRegion(checkedRect.Left, checkedRect.Top, 0, checkedRect.Right, checkedRect.Bottom, 1);
                d3dContext.CopySubresourceRegion(this.GetTexture(), subresourceIndex, region, stagingTexture, 0);

                // Copy the data to the array.
                DX.DataStream stream = null;
                try
                {
                    DX.DataBox databox = d3dContext.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

                    int elementSize = this.Format.GetSize();
                    if (this.Format.IsCompressedFormat())
                    {
                        // for 4x4 block compression formats an element is one block, so elementsInRow
                        // and number of rows are 1/4 of number of pixels in width and height of the rectangle
                        elementsInRow /= 4;
                        rows /= 4;
                    }
                    int rowSize = elementSize * elementsInRow;
                    if (rowSize == databox.RowPitch)
                        stream.ReadRange(data, startIndex, elementCount);
                    else if (level == 0 && arraySlice == 0 &&
                             checkedRect.X == 0 && checkedRect.Y == 0 &&
                             checkedRect.Width == this.Width && checkedRect.Height == this.Height &&
                             startIndex == 0 && elementCount == data.Length)
                    {
                        // TNC: optimized PlatformGetData() that reads multiple elements in a row when texture has rowPitch
                        int elementSize2 = DX.Utilities.SizeOf<T>();
                        if (elementSize2 == 1) // byte[]
                            elementsInRow = elementsInRow * elementSize;

                        int currentIndex = 0;
                        for (int row = 0; row < rows; row++)
                        {
                            stream.ReadRange(data, currentIndex, elementsInRow);
                            stream.Seek((databox.RowPitch - rowSize), SeekOrigin.Current);
                            currentIndex += elementsInRow;
                        }
                    }
                    else
                    {
                        // Some drivers may add pitch to rows.
                        // We need to copy each row separatly and skip trailing zeros.
                        stream.Seek(0, SeekOrigin.Begin);

                        int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                        for (int row = 0; row < rows; row++)
                        {
                            int i;
                            int maxElements =  (row + 1) * rowSize / elementSizeInByte;
                            for (i = row * rowSize / elementSizeInByte; i < maxElements; i++)
                                data[i + startIndex] = stream.Read<T>();

                            if (i >= elementCount)
                                break;

                            stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                        }
                    }
                }
                finally
                {
                    DX.Utilities.Dispose( ref stream);

                    d3dContext.UnmapSubresource(stagingTexture, 0);                    
                    DX.Utilities.Dispose(ref stagingTexture);
                }
            }
        }

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            int blockWidth, blockHeight;
            Format.GetBlockSize(out blockWidth, out blockHeight);
            int blockWidthMinusOne = blockWidth - 1;
            int blockHeightMinusOne = blockHeight - 1;
            // round x and y down to next multiple of block size; width and height up to next multiple of block size
            int roundedWidth = (rect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
            int roundedHeight = (rect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
            checkedRect = new Rectangle(rect.X & ~blockWidthMinusOne, rect.Y & ~blockHeightMinusOne,
                                        roundedWidth, roundedHeight);
            if (Format == SurfaceFormat.RgbPvrtc2Bpp || Format == SurfaceFormat.RgbaPvrtc2Bpp)
            {
                return (Math.Max(checkedRect.Width, 16) * Math.Max(checkedRect.Height, 8) * 2 + 7) / 8;
            }
            else if (Format == SurfaceFormat.RgbPvrtc4Bpp || Format == SurfaceFormat.RgbaPvrtc4Bpp)
            {
                return (Math.Max(checkedRect.Width, 8) * Math.Max(checkedRect.Height, 8) * 4 + 7) / 8;
            }
            else
            {
                return roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight);
            }
        }
        #endregion ITexture2DStrategy


        internal void PlatformConstructTexture2D(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, SurfaceFormat format, bool shared)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Width;
            texture2DDesc.Height = this.Height;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = this.ArraySize;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            if (this._shared)
                texture2DDesc.OptionFlags |= D3D11.ResourceOptionFlags.Shared;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

    }
}
