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
using MonoGame.Framework.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteIndexBuffer : IndexBufferStrategy
    {
        internal bool _isDynamic;

        private D3D11.Buffer _buffer;

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
            this._isDynamic = isDynamic;
        }

        internal ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            this._isDynamic = false;

            PlatformConstructIndexBuffer();
        }

        internal void PlatformConstructIndexBuffer()
        {
            Debug.Assert(_buffer == null);

            // TODO: To use true Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            int sizeInBytes = this.IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = sizeInBytes;
            bufferDesc.Usage = D3D11.ResourceUsage.Default;
            bufferDesc.BindFlags = D3D11.BindFlags.IndexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            if (_isDynamic)
            {
                bufferDesc.CpuAccessFlags |= D3D11.CpuAccessFlags.Write;
                bufferDesc.Usage = D3D11.ResourceUsage.Dynamic;
            }

            _buffer = new D3D11.Buffer(this.GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
           Debug.Assert(_buffer != null);

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
                    DX.Utilities.Write(IntPtr.Add(dataBox.DataPointer, offsetInBytes), data, startIndex,
                                            elementCount);
                    d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
                int elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    int startBytes = startIndex * elementSizeInBytes;
                    IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

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
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            Debug.Assert(_buffer != null);

            if (_isDynamic)
            {
                throw new NotImplementedException();
            }
            else
            {

                // Copy the texture to a staging resource
                D3D11.BufferDescription stagingDesc = _buffer.Description;
                stagingDesc.BindFlags = D3D11.BindFlags.None;
                stagingDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
                stagingDesc.Usage = D3D11.ResourceUsage.Staging;
                stagingDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
                using (D3D11.Buffer stagingBuffer = new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, stagingDesc))
                {
                    lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                    {
                        D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                        d3dContext.CopyResource(_buffer, stagingBuffer);
                    }

                    int TsizeInBytes = DX.Utilities.SizeOf<T>();
                    GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    try
                    {
                        int startBytes = startIndex * TsizeInBytes;
                        IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                        DX.DataPointer DataPointer = new DX.DataPointer(dataPtr, elementCount * TsizeInBytes);

                        lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
                        {
                            D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                            // Map the staging resource to a CPU accessible memory
                            DX.DataBox box = d3dContext.MapSubresource(stagingBuffer, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

                            DX.Utilities.CopyMemory(dataPtr, box.DataPointer + offsetInBytes, elementCount * TsizeInBytes);

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
        }

        internal override void PlatformGraphicsDeviceResetting()
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
