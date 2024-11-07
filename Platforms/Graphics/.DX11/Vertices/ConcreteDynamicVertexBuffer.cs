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
using Microsoft.Xna.Platform.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteDynamicVertexBuffer : ConcreteVertexBuffer, IDynamicVertexBufferStrategy
    {
        private bool _isContentLost;

        internal ConcreteDynamicVertexBuffer(GraphicsContextStrategy contextStrategy, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
            : base(contextStrategy, vertexDeclaration, vertexCount, usage, isDynamic:true)
        {
            PlatformConstructDynamicVertexBuffer();
        }

        private void PlatformConstructDynamicVertexBuffer()
        {
            Debug.Assert(_buffer == null);

            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.SizeInBytes = this.VertexDeclaration.VertexStride * this.VertexCount;
            bufferDesc.Usage = D3D11.ResourceUsage.Dynamic;
            bufferDesc.BindFlags = D3D11.BindFlags.VertexBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.Write;
            bufferDesc.OptionFlags = D3D11.ResourceOptionFlags.None;
            bufferDesc.StructureByteStride = 0;// StructureSizeInBytes

            _buffer = new D3D11.Buffer(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
        }

        public unsafe override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
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

                int TsizeInBytes = sizeof(T);
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                    dataPtr = dataPtr + startIndex * TsizeInBytes;

                    IntPtr dstPtr = dataBox.DataPointer + offsetInBytes;
                    if (vertexStride == elementSizeInBytes)
                    {
                        MemCopyHelper.MemoryCopy(
                            dataPtr,
                            dstPtr,
                            elementCount * elementSizeInBytes);
                    }
                    else
                    {
                        for (int i = 0; i < elementCount; i++)
                            MemCopyHelper.MemoryCopy(
                                dataPtr + i * elementSizeInBytes,
                                dstPtr  + i * vertexStride,
                                elementSizeInBytes);
                    }
                }
                finally
                {
                    d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride)
        {
            Debug.Assert(_buffer != null);

            throw new NotImplementedException();
        }


        #region IDynamicVertexBufferStrategy
        public bool IsContentLost
        {
            get { return _isContentLost; }
        }
        #endregion IDynamicVertexBufferStrategy

    }

}
