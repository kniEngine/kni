// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteIndexBuffer : IndexBufferStrategy
    {
        private readonly DXGI.Format _drawElementsType;
        internal D3D11.Buffer _buffer;

        internal DXGI.Format DrawElementsType { get { return _drawElementsType; } }
        internal D3D11.Buffer DXIndexBuffer
        {
            get
            {
                Debug.Assert(_buffer != null);
                return _buffer;
            }
        }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            Debug.Assert(isDynamic == true);

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits: _drawElementsType = DXGI.Format.R16_UInt; break;
                case IndexElementSize.ThirtyTwoBits: _drawElementsType = DXGI.Format.R32_UInt; break;
                default: throw new InvalidOperationException();
            }
        }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits: _drawElementsType = DXGI.Format.R16_UInt; break;
                case IndexElementSize.ThirtyTwoBits: _drawElementsType = DXGI.Format.R32_UInt; break;
                default: throw new InvalidOperationException();
            }

            PlatformConstructIndexBuffer();
        }

        internal void PlatformConstructIndexBuffer()
        {
            Debug.Assert(_buffer == null);

            int sizeInBytes = this.IndexCount * base.ElementSizeInBytes;

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = sizeInBytes;
            bufferDesc.Usage = D3D11.ResourceUsage.Default;
            bufferDesc.BindFlags = D3D11.BindFlags.IndexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            _buffer = new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            Debug.Assert(_buffer != null);

            int elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                dataPtr = new IntPtr(dataPtr.ToInt64() + startIndex * elementSizeInBytes);

                DX.DataBox dataBox = new DX.DataBox(dataPtr, elementCount * elementSizeInBytes, 0);

                D3D11.ResourceRegion region = new D3D11.ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = 1;
                region.Left = offsetInBytes;
                region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.UpdateSubresource(dataBox, _buffer, 0, region);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            Debug.Assert(_buffer != null);

            // Copy the texture to a staging resource
            D3D11.BufferDescription stagingDesc = _buffer.Description;
            stagingDesc.BindFlags = D3D11.BindFlags.None;
            stagingDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
            stagingDesc.Usage = D3D11.ResourceUsage.Staging;
            stagingDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            using (D3D11.Buffer stagingBuffer = new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, stagingDesc))
            {
                lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                {
                    D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    d3dContext.CopyResource(_buffer, stagingBuffer);
                }

                int TsizeInBytes = DX.Utilities.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                    dataPtr = new IntPtr(dataPtr.ToInt64() + startIndex * TsizeInBytes);

                    lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
                    {
                        D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                        // Map the staging resource to a CPU accessible memory
                        DX.DataBox dataBox = d3dContext.MapSubresource(stagingBuffer, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

                        DX.Utilities.CopyMemory(dataPtr, dataBox.DataPointer + offsetInBytes, elementCount * TsizeInBytes);

                        // Make sure that we unmap the resource in case of an exception
                        d3dContext.UnmapSubresource(stagingBuffer, 0);
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            DX.Utilities.Dispose(ref _buffer);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _buffer);
            }

            base.Dispose(disposing);
        }
    }

}
