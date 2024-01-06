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
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteVertexBuffer : VertexBufferStrategy
    {
        internal D3D11.Buffer _buffer;

        internal D3D11.Buffer DXVertexBuffer
        {
            get
            {
                Debug.Assert(_buffer != null);
                return _buffer;
            }
        }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            Debug.Assert(isDynamic == true);
        }

        internal ConcreteVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage)
        {
            PlatformConstructVertexBuffer();
        }


        internal void PlatformConstructVertexBuffer()
        {
            Debug.Assert(_buffer == null);

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = this.VertexDeclaration.VertexStride * this.VertexCount;
            bufferDesc.Usage = D3D11.ResourceUsage.Default;
            bufferDesc.BindFlags = D3D11.BindFlags.VertexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            _buffer = new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }

        internal D3D11.Buffer CreateStagingBuffer()
        {
            D3D11.BufferDescription stagingDesc = _buffer.Description;
            stagingDesc.BindFlags = D3D11.BindFlags.None;
            stagingDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
            stagingDesc.Usage = D3D11.ResourceUsage.Staging;
            stagingDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            return new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, stagingDesc);
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
            Debug.Assert(_buffer != null);

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                int startBytes = startIndex * elementSizeInBytes;
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                if (vertexStride == elementSizeInBytes)
                {
                    DX.DataBox box = new DX.DataBox(dataPtr, elementCount * elementSizeInBytes, 0);

                    D3D11.ResourceRegion region = new D3D11.ResourceRegion();
                    region.Top = 0;
                    region.Front = 0;
                    region.Back = 1;
                    region.Bottom = 1;
                    region.Left = offsetInBytes;
                    region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                    lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
                    {
                        D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                        d3dContext.UpdateSubresource(box, _buffer, 0, region);
                    }
                }
                else
                {
                    using (D3D11.Buffer stagingBuffer = CreateStagingBuffer())
                        lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
                        {
                            D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                            d3dContext.CopyResource(_buffer, stagingBuffer);

                            // Map the staging resource to a CPU accessible memory
                            DX.DataBox box = d3dContext.MapSubresource(stagingBuffer, 0, D3D11.MapMode.Read,
                            D3D11.MapFlags.None);

                            for (int i = 0; i < elementCount; i++)
                                DX.Utilities.CopyMemory(
                                box.DataPointer + i * vertexStride + offsetInBytes,
                                dataPtr + i * elementSizeInBytes, elementSizeInBytes);

                            // Make sure that we unmap the resource in case of an exception
                            d3dContext.UnmapSubresource(stagingBuffer, 0);

                            // Copy back from staging resource to real buffer.
                            d3dContext.CopyResource(stagingBuffer, _buffer);
                        }
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            Debug.Assert(_buffer != null);

            int TsizeInBytes = DX.Utilities.SizeOf<T>();
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                int startBytes = startIndex * TsizeInBytes;
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                using (D3D11.Buffer stagingBuffer = CreateStagingBuffer())
                    lock (base.GraphicsDeviceStrategy.CurrentContext.Strategy.SyncHandle)
                    {
                        D3D11.DeviceContext d3dContext = base.GraphicsDeviceStrategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                        d3dContext.CopyResource(_buffer, stagingBuffer);

                        // Map the staging resource to a CPU accessible memory
                        DX.DataBox box = d3dContext.MapSubresource(stagingBuffer, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

                        if (vertexStride == TsizeInBytes)
                        {
                            DX.Utilities.CopyMemory(dataPtr, box.DataPointer + offsetInBytes, vertexStride * elementCount);
                        }
                        else
                        {
                            for (int i = 0; i < elementCount; i++)
                                DX.Utilities.CopyMemory(dataPtr + i * TsizeInBytes, box.DataPointer + i * vertexStride + offsetInBytes, TsizeInBytes);
                        }

                        // Make sure that we unmap the resource in case of an exception
                        d3dContext.UnmapSubresource(stagingBuffer, 0);
                    }
            }
            finally
            {
                dataHandle.Free();
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
