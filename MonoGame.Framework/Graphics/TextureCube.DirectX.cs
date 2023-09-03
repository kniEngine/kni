// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Platform.Graphics;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private bool _renderTarget;
        private bool _mipMap;

        private void PlatformConstructTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            _renderTarget = renderTarget;
            _mipMap = mipMap;

            // Create texture
            GetTexture();
        }

        internal override D3D11.Resource CreateTexture()
        {
            D3D11.Texture2DDescription description = new D3D11.Texture2DDescription
            {
                Width = this.Size,
                Height = this.Size,
                MipLevels = this.LevelCount,
                ArraySize = 6, // A texture cube is a 2D texture array with 6 textures.
                Format = GraphicsExtensions.ToDXFormat(this.Format),
                BindFlags = D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = D3D11.ResourceUsage.Default,
                OptionFlags = D3D11.ResourceOptionFlags.TextureCube
            };

            if (_renderTarget)
            {
                description.BindFlags |= D3D11.BindFlags.RenderTarget;
                if (_mipMap)
                    description.OptionFlags |= D3D11.ResourceOptionFlags.GenerateMipMaps;
            }

            return new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, description);
        }

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            // Create a temp staging resource for copying the data.
            // 
            // TODO: Like in Texture2D, we should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var min = this.Format.IsCompressedFormat() ? 4 : 1;
            var levelSize = Math.Max(this.Size >> level, min);

            D3D11.Texture2DDescription desc = new D3D11.Texture2DDescription
            {
                Width = levelSize,
                Height = levelSize,
                MipLevels = 1,
                ArraySize = 1,
                Format = GraphicsExtensions.ToDXFormat(this.Format),
                SampleDescription = new DXGI.SampleDescription(1, 0),
                BindFlags = D3D11.BindFlags.None,
                CpuAccessFlags = D3D11.CpuAccessFlags.Read,
                Usage = D3D11.ResourceUsage.Staging,
                OptionFlags = D3D11.ResourceOptionFlags.None,
            };

            using (D3D11.Texture2D stagingTex = new D3D11.Texture2D(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, desc))
            {
                lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                {
                    D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    // Copy the data from the GPU to the staging texture.
                    var subresourceIndex = CalculateSubresourceIndex(cubeMapFace, level);
                    var elementsInRow = rect.Width;
                    var rows = rect.Height;
                    D3D11.ResourceRegion region = new D3D11.ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1);
                    d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, region, stagingTex, 0);

                    // Copy the data to the array.
                    DataStream stream = null;
                    try
                    {
                        var databox = d3dContext.MapSubresource(stagingTex, 0, D3D11.MapMode.Read, D3D11.MapFlags.None, out stream);

                        var elementSize = this.Format.GetSize();
                        if (this.Format.IsCompressedFormat())
                        {
                            // for 4x4 block compression formats an element is one block, so elementsInRow
                            // and number of rows are 1/4 of number of pixels in width and height of the rectangle
                            elementsInRow /= 4;
                            rows /= 4;
                        }
                        var rowSize = elementSize * elementsInRow;
                        if (rowSize == databox.RowPitch)
                            stream.ReadRange(data, startIndex, elementCount);
                        else
                        {
                            // Some drivers may add pitch to rows.
                            // We need to copy each row separatly and skip trailing zeros.
                            stream.Seek(0, SeekOrigin.Begin);

                            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
                            for (var row = 0; row < rows; row++)
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
                        SharpDX.Utilities.Dispose(ref stream);
                    }
                }
            }
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
        {
            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var dataPtr = (IntPtr) (dataHandle.AddrOfPinnedObject().ToInt64() + startIndex*elementSizeInByte);
                var box = new DataBox(dataPtr, Texture.GetPitch(this.Format, rect.Width), 0);

                var subresourceIndex = CalculateSubresourceIndex(face, level);

                D3D11.ResourceRegion region = new D3D11.ResourceRegion
                {
                    Top = rect.Top,
                    Front = 0,
                    Back = 1,
                    Bottom = rect.Bottom,
                    Left = rect.Left,
                    Right = rect.Right
                };

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

	    private int CalculateSubresourceIndex(CubeMapFace face, int level)
	    {
	        return (int) face * this.LevelCount + level;
	    }
	}
}

