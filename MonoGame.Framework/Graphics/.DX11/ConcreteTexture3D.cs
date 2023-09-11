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
            : this(contextStrategy, width, height, depth, mipMap, format)
        {
            this._width = width;
            this._height = height;
            this._depth = depth;

            this._mipMap = mipMap;

            System.Diagnostics.Debug.Assert(isRenderTarget);
        }

        internal ConcreteTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : base(contextStrategy, format, Texture.CalculateMipLevels(mipMap, width, height, depth))
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

        public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                int rowPitch = Texture.GetPitch(this.Format, width);
                int slicePitch = rowPitch * height; // For 3D texture: Size of 2D image.
                DX.DataBox box = new DX.DataBox(dataPtr, rowPitch, slicePitch);

                int subresourceIndex = level;

                D3D11.ResourceRegion region = new D3D11.ResourceRegion(left, top, front, right, bottom, back);

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

        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                               T[] data, int startIndex, int elementCount)
             where T : struct
        {
            // Create a temp staging resource for copying the data.
            //
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = 1;
            texture3DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture3DDesc.BindFlags = D3D11.BindFlags.None;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read;
            texture3DDesc.Usage = D3D11.ResourceUsage.Staging;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            using (D3D11.Texture3D stagingTex = new D3D11.Texture3D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc))
            {
                lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                {
                    D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    // Copy the data from the GPU to the staging texture.
                    d3dContext.CopySubresourceRegion(this.GetTexture(), level, new D3D11.ResourceRegion(left, top, front, right, bottom, back), stagingTex, 0);

                    // Copy the data to the array.
                    DX.DataStream stream = null;
                    try
                    {
                        DX.DataBox databox = d3dContext.MapSubresource(stagingTex, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

                        // Some drivers may add pitch to rows or slices.
                        // We need to copy each row separatly and skip trailing zeros.
                        int currentIndex = startIndex;
                        int elementSize = this.Format.GetSize();
                        int elementsInRow = right - left;
                        int rowsInSlice = bottom - top;
                        for (int slice = front; slice < back; slice++)
                        {
                            for (int row = top; row < bottom; row++)
                            {
                                stream.ReadRange(data, currentIndex, elementsInRow);
                                stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                                currentIndex += elementsInRow;
                            }
                            stream.Seek(databox.SlicePitch - (databox.RowPitch * rowsInSlice), SeekOrigin.Current);
                        }
                    }
                    finally
                    {
                        DX.Utilities.Dispose(ref stream);
                    }
                }
            }

        }
        #endregion #region ITexture3DStrategy

        internal void PlatformConstructTexture3D(GraphicsContextStrategy contextStrategy, int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            D3D11.Texture3DDescription texture3DDesc = new D3D11.Texture3DDescription();
            texture3DDesc.Width = this.Width;
            texture3DDesc.Height = this.Height;
            texture3DDesc.Depth = this.Depth;
            texture3DDesc.MipLevels = this.LevelCount;
            texture3DDesc.Format = GraphicsExtensions.ToDXFormat(this.Format);
            texture3DDesc.BindFlags = D3D11.BindFlags.ShaderResource;
            texture3DDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            texture3DDesc.Usage = D3D11.ResourceUsage.Default;
            texture3DDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            D3D11.Resource texture = new D3D11.Texture3D(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
            _texture = texture;
            _resourceView = new D3D11.ShaderResourceView(contextStrategy.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture);
        }

    }
}
