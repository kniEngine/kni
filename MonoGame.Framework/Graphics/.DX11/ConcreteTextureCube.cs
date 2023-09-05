// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteTextureCube : ConcreteTexture, ITextureCubeStrategy
    {
        private readonly int _size;

        internal ConcreteTextureCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, size))
        {
            this._size = size;
        }


        #region ITextureCubeStrategy
        public int Size { get { return _size; } }

        public void SetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                IntPtr dataPtr = (IntPtr) (dataHandle.AddrOfPinnedObject().ToInt64() + startIndex*elementSizeInByte);
                DX.DataBox box = new DX.DataBox(dataPtr, Texture.GetPitch(this.Format, checkedRect.Width), 0);

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

                lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                {
                    D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(box, this.GetTexture(), subresourceIndex, region);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public void GetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
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
            texture2DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture2DDesc.SampleDescription = sampleDesc;
            texture2DDesc.BindFlags = D3D11.BindFlags.None;
            texture2DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            texture2DDesc.Usage = D3D11.ResourceUsage.Staging;
            texture2DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            using (D3D11.Texture2D stagingTex = new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture2DDesc))
            {
                lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                {
                    D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    // Copy the data from the GPU to the staging texture.
                    int subresourceIndex = (int)face * this.LevelCount + level;
                    int elementsInRow = checkedRect.Width;
                    int rows = checkedRect.Height;
                    D3D11.ResourceRegion region = new D3D11.ResourceRegion(checkedRect.Left, checkedRect.Top, 0, checkedRect.Right, checkedRect.Bottom, 1);
                    d3dContext.CopySubresourceRegion(this.GetTexture(), subresourceIndex, region, stagingTex, 0);

                    // Copy the data to the array.
                    DX.DataStream stream = null;
                    try
                    {
                        DX.DataBox databox = d3dContext.MapSubresource(stagingTex, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

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
                        else
                        {
                            // Some drivers may add pitch to rows.
                            // We need to copy each row separatly and skip trailing zeros.
                            stream.Seek(0, SeekOrigin.Begin);

                            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                            for (int row = 0; row < rows; row++)
                            {
                                int i;
                                for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
                                    data[i + startIndex] = stream.Read<T>();

                                if (i >= elementCount)
                                    break;

                                stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                            }
                        }
                    }
                    finally
                    {
                        DX.Utilities.Dispose(ref stream);
                    }
                }
            }

        }
        #endregion #region ITextureCubeStrategy


        internal bool _mipMap;
        internal bool _isRenderTarget;
    }
}
