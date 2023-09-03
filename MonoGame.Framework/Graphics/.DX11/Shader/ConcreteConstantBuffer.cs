// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {
        internal D3D11.Buffer _cbuffer;


        public ConcreteConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes)
            : base(graphicsDevice, name, parameters, offsets, sizeInBytes)
        {
            _cbuffer = CreateD3D11Buffer();
        }

        private ConcreteConstantBufferStrategy(ConcreteConstantBufferStrategy source)
            : base(source)
        {
            _cbuffer = CreateD3D11Buffer();
        }

        public override object Clone()
        {
            return new ConcreteConstantBufferStrategy(this);
        }

        private D3D11.Buffer CreateD3D11Buffer()
        {
            // Allocate the hardware constant buffer.
            D3D11.BufferDescription desc = new D3D11.BufferDescription();
            desc.Usage = D3D11.ResourceUsage.Default;
            desc.BindFlags = D3D11.BindFlags.ConstantBuffer;
            desc.CpuAccessFlags = D3D11.CpuAccessFlags.None;
            desc.SizeInBytes = Buffer.Length;

            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                return new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, desc);
            }
        }

        internal override void PlatformClear()
        {
            DX.Utilities.Dispose(ref _cbuffer);
            Dirty = true;
        }

        internal unsafe override void PlatformApply(GraphicsContextStrategy contextStrategy, ShaderStage stage, int slot)
        {
            if (_cbuffer == null)
                _cbuffer = CreateD3D11Buffer();

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
