// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        private D3D11.Buffer _buffer;

        internal D3D11.Buffer Buffer
        {
            get
            {
                if (_buffer != null)
                    return _buffer;
                
                GenerateIfRequired();
                return _buffer;
            }
        }

        private void PlatformConstructVertexBuffer()
        {
            GenerateIfRequired();
        }

        void GenerateIfRequired()
        {
            if (_buffer != null)
                return;

            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = VertexDeclaration.VertexStride * VertexCount;
            bufferDesc.Usage = D3D11.ResourceUsage.Default;
            bufferDesc.BindFlags = D3D11.BindFlags.VertexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            if (_isDynamic)
            {
                bufferDesc.CpuAccessFlags |= D3D11.CpuAccessFlags.Write;
                bufferDesc.Usage = D3D11.ResourceUsage.Dynamic;
            }

            _buffer = new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }

        D3D11.Buffer CreateStagingBuffer()
        {
            D3D11.BufferDescription stagingDesc = _buffer.Description;
            stagingDesc.BindFlags = D3D11.BindFlags.None;
            stagingDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
            stagingDesc.Usage = D3D11.ResourceUsage.Staging;
            stagingDesc.OptionFlags = D3D11.ResourceOptionFlags.None;

            return new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, stagingDesc);
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                throw new NotImplementedException();
            }
            else
            {
                int TsizeInBytes = DX.Utilities.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

                try
                {
                    int startBytes = startIndex * TsizeInBytes;
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    using (D3D11.Buffer stagingBuffer = CreateStagingBuffer())
                    lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                    {
                        D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

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
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                // We assume discard by default.
                D3D11.MapMode mode = D3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = D3D11.MapMode.WriteNoOverwrite;

                lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                {
                    D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                    DX.DataBox dataBox = d3dContext.MapSubresource(_buffer, 0, mode, D3D11.MapFlags.None);
                    if (vertexStride == elementSizeInBytes)
					{
                        DX.Utilities.Write(dataBox.DataPointer + offsetInBytes, data, startIndex, elementCount);
                    }
                    else
                    {
                        for (int i = 0; i < elementCount; i++)
                            DX.Utilities.Write(dataBox.DataPointer + offsetInBytes + i * vertexStride, data, startIndex + i, 1);
                    }

                    d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
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

                        lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                        {
                            D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                            d3dContext.UpdateSubresource(box, _buffer, 0, region);
                        }
                    }
                    else
                    {
                        using(D3D11.Buffer stagingBuffer = CreateStagingBuffer())
                        lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                        {
                            D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

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
        }

        private void PlatformGraphicsDeviceResetting()
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
