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
    public class ConcreteDynamicIndexBuffer : ConcreteIndexBuffer, IDynamicIndexBufferStrategy
    {
        private bool _isContentLost;

        internal ConcreteDynamicIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic:true)
        {
            PlatformConstructDynamicIndexBuffer();
        }
        
        private void PlatformConstructDynamicIndexBuffer()
        {
            Debug.Assert(_buffer == null);

            int sizeInBytes = this.IndexCount * base.ElementSizeInBytes;

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = sizeInBytes;
            bufferDesc.Usage = D3D11.ResourceUsage.Dynamic;
            bufferDesc.BindFlags = D3D11.BindFlags.IndexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Write;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            _buffer = new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }


        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            Debug.Assert(_buffer != null);

            // We assume discard by default.
            D3D11.MapMode mode = D3D11.MapMode.WriteDiscard;
            if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                mode = D3D11.MapMode.WriteNoOverwrite;

            lock (((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                DX.DataBox dataBox = d3dContext.MapSubresource(_buffer, 0, mode, D3D11.MapFlags.None);
                try
                {
                    IntPtr dstPtr = dataBox.DataPointer;
                    DX.Utilities.Write(dstPtr + offsetInBytes, data, startIndex,
                                       elementCount);
                }
                finally
                {
                    d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            Debug.Assert(_buffer != null);

            throw new NotImplementedException();
        }


        #region IDynamicIndexBufferStrategy
        public bool IsContentLost
        {
            get { return _isContentLost; }
        }
        #endregion IDynamicIndexBufferStrategy

    }

}
