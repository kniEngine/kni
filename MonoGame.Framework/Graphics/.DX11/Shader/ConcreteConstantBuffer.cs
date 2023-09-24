// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{

    internal sealed class ConcreteConstantBuffer : ConstantBufferStrategy
    {
        internal D3D11.Buffer _cbuffer;


        public ConcreteConstantBuffer(GraphicsContextStrategy contextStrategy, string name, int[] parameters, int[] offsets, int sizeInBytes, ShaderProfileType profile)
            : base(contextStrategy, name, parameters, offsets, sizeInBytes, profile)
        {
            if (profile != ShaderProfileType.DirectX_11)
                throw new Exception("This effect was built for a different platform.");

            _cbuffer = CreateD3D11Buffer();
        }

        private ConcreteConstantBuffer(ConcreteConstantBuffer source)
            : base(source)
        {
            _cbuffer = CreateD3D11Buffer();
        }

        public override object Clone()
        {
            return new ConcreteConstantBuffer(this);
        }

        private D3D11.Buffer CreateD3D11Buffer()
        {
            // Allocate the hardware constant buffer.
            D3D11.BufferDescription bufferDesc = new D3D11.BufferDescription();
            bufferDesc.Usage = D3D11.ResourceUsage.Default;
            bufferDesc.BindFlags = D3D11.BindFlags.ConstantBuffer;
            bufferDesc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            bufferDesc.SizeInBytes = Buffer.Length;

            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                return new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
            }
        }

        internal unsafe override void PlatformApply(GraphicsContextStrategy contextStrategy, int slot, ShaderStage stage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the CurrentD3DContext for us to use.

            // Update the hardware buffer.
            if (Dirty)
            {
                contextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.UpdateSubresource(Buffer, _cbuffer);
                Dirty = false;
            }

            // Set the buffer to the right stage.
            switch (stage)
            {
                case ShaderStage.Pixel: contextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.PixelShader.SetConstantBuffer(slot, _cbuffer); break;
                case ShaderStage.Vertex: contextStrategy.ToConcrete<ConcreteGraphicsContext>().D3dContext.VertexShader.SetConstantBuffer(slot, _cbuffer); break;
                default: throw new System.ArgumentException();
            }
        }

        internal override void PlatformDeviceResetting()
        {
            DX.Utilities.Dispose(ref _cbuffer);
            Dirty = true;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _cbuffer);
            }

            base.Dispose(disposing);
        }
    }
}
