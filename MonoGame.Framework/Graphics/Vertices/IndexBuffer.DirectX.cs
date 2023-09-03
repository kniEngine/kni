// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        private D3D11.Buffer _buffer;

        internal D3D11.Buffer Buffer
        {
            get
            {
                GenerateIfRequired();
                return _buffer;
            }
        }

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {
            GenerateIfRequired();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            DX.Utilities.Dispose(ref _buffer);
        }

        void GenerateIfRequired()
        {
            if (_buffer != null)
                return;

            // TODO: To use true Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            int sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            D3D11.CpuAccessFlags accessflags = D3D11.CpuAccessFlags.None;
            D3D11.ResourceUsage resUsage = D3D11.ResourceUsage.Default;

            if (_isDynamic)
            {
                accessflags |= D3D11.CpuAccessFlags.Write;
                resUsage = D3D11.ResourceUsage.Dynamic;
            }

            _buffer = new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice,
                                                        sizeInBytes,
                                                        resUsage,
                                                        D3D11.BindFlags.IndexBuffer,
                                                        accessflags,
                                                        D3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GenerateIfRequired();

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

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DX.Utilities.Dispose(ref _buffer);

            base.Dispose(disposing);
        }
	}
}
