// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private void PlatformConstructTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            ((ConcreteTexture3D)_strategyTexture3D)._isRenderTarget = renderTarget;
            ((ConcreteTexture3D)_strategyTexture3D)._mipMap = mipMap;

            // Create texture
            GetTexture();
        }


        internal override D3D11.Resource CreateTexture()
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

            if (((ConcreteTexture3D)_strategyTexture3D)._isRenderTarget)
            {
                texture3DDesc.BindFlags |= D3D11.BindFlags.RenderTarget;
                if (((ConcreteTexture3D)_strategyTexture3D)._mipMap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    texture3DDesc.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
                }
            }

            return new D3D11.Texture3D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, texture3DDesc);
        }

	    private void PlatformSetData<T>(int level,
                                     int left, int top, int right, int bottom, int front, int back,
                                     T[] data, int startIndex, int elementCount, int width, int height, int depth)
        {
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

                    d3dContext.UpdateSubresource(box, GetTexture(), subresourceIndex, region);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
             where T : struct
        {

            // Create a temp staging resource for copying the data.
            // 
            // TODO: Like in Texture2D, we should probably be pooling these staging resources
            // and not creating a new one each time.
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
                    d3dContext.CopySubresourceRegion(GetTexture(), level, new D3D11.ResourceRegion(left, top, front, right, bottom, back), stagingTex, 0);

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
	}
}

