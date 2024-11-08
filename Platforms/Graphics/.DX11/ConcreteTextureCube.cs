// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;

        internal readonly bool _mipMap;


        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format,
                                     bool isRenderTarget)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            this._mipMap = mipMap;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, TextureHelpers.CalculateMipLevels(mipMap, size))
        {
            this._size = size;

            this._mipMap = mipMap;

            this.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public unsafe void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int elementSizeInByte = sizeof(T);
            fixed (T* pData = &data[0])
            {
                IntPtr dataPtr = (IntPtr)pData;
                dataPtr = dataPtr + startIndex * elementSizeInByte;

                DX.DataBox dataBox = new DX.DataBox(dataPtr, this.Format.GetPitch(checkedRect.Width), 0);

                int subresourceIndex = (int)face * this.LevelCount + level;

                D3D11.ResourceRegion region = new D3D11.ResourceRegion
                {
                    Top = checkedRect.Top,
                    Front = 0,
                    Back = 1,
                    Bottom = checkedRect.Bottom,
                    Left = checkedRect.Left,
                    Right = checkedRect.Right
                };

                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(dataBox, this.GetTexture(), subresourceIndex, region);
                }
            }
        }

        public unsafe void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            // Create a temp staging resource for copying the data.
            //
            int min = this.Format.IsCompressedFormat() ? 4 : 1;
            int levelSize = Math.Max(this.Size >> level, min);

            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = levelSize;
            texture2DDesc.Height = levelSize;
            texture2DDesc.MipLevels = 1;
            texture2DDesc.ArraySize = 1;
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.BindFlags = D3D11.BindFlags.None;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            texture2DDesc.Usage = D3D11.ResourceUsage.Staging;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            using (D3D11.Texture2D stagingTexture = new D3D11.Texture2D(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc))
            {
                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    // Copy the data from the GPU to the staging texture.
                    int subresourceIndex = (int)face * this.LevelCount + level;
                    int elementsInRow = checkedRect.Width;
                    int rows = checkedRect.Height;
                    D3D11.ResourceRegion region = new D3D11.ResourceRegion(checkedRect.Left, checkedRect.Top, 0, checkedRect.Right, checkedRect.Bottom, 1);
                    d3dContext.CopySubresourceRegion(this.GetTexture(), subresourceIndex, region, stagingTexture, 0);

                    // Copy the data to the array.
                    DX.DataStream stream = null;
                    DX.DataBox dataBox = d3dContext.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);
                    try
                    {
                        int elementSize = this.Format.GetSize();
                        if (this.Format.IsCompressedFormat())
                        {
                            // for 4x4 block compression formats an element is one block, so elementsInRow
                            // and number of rows are 1/4 of number of pixels in width and height of the rectangle
                            elementsInRow /= 4;
                            rows /= 4;
                        }
                        int rowSize = elementSize * elementsInRow;
                        if (rowSize == dataBox.RowPitch)
                            stream.ReadRange(data, startIndex, elementCount);
                        else
                        {
                            // Some drivers may add pitch to rows.
                            // We need to copy each row separatly and skip trailing zeros.
                            stream.Seek(0, SeekOrigin.Begin);

                            int elementSizeInByte = sizeof(T);
                            for (int row = 0; row < rows; row++)
                            {
                                int i;
                                for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
                                    data[i + startIndex] = stream.Read<T>();

                                if (i >= elementCount)
                                    break;

                                stream.Seek(dataBox.RowPitch - rowSize, SeekOrigin.Current);
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

        public int GetCompressedDataByteSize(int fSize, Rectangle rect, ref Rectangle textureBounds, out Rectangle checkedRect)
        {
            // round x and y down to next multiple of four; width and height up to next multiple of four
            int roundedWidth = (rect.Width + 3) & ~0x3;
            int roundedHeight = (rect.Height + 3) & ~0x3;
            checkedRect = new Rectangle(rect.X & ~0x3, rect.Y & ~0x3,
                                        roundedWidth, roundedHeight);
            return (roundedWidth * roundedHeight * fSize / 16);
        }
        #endregion ITextureCubeStrategy

        internal void PlatformConstructTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            DXGI.SampleDescription sampleDesc = new DXGI.SampleDescription(1, 0);
            D3D11.Texture2DDescription texture2DDesc = new D3D11.Texture2DDescription();
            texture2DDesc.Width = this.Size;
            texture2DDesc.Height = this.Size;
            texture2DDesc.MipLevels = this.LevelCount;
            texture2DDesc.ArraySize = 6; // A texture cube is a 2D texture array with 6 textures.
            texture2DDesc.Format = this.Format.ToDXFormat();
            texture2DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.Usage = D3D11.ResourceUsage.Default;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;

            System.Diagnostics.Debug.Assert(_texture == null);
            D3D11.Resource texture = new D3D11.Texture2D(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }


    }
}
