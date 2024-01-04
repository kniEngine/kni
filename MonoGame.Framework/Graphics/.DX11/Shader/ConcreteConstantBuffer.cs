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
        private D3D11.Buffer _cbuffer;

        internal D3D11.Buffer DXcbuffer { get { return _cbuffer; } }


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
            bufferDesc.SizeInBytes = this.BufferData.Length;

            lock (GraphicsDevice.Strategy.CurrentContext.Strategy.SyncHandle)
            {
                D3D11.DeviceContext d3dContext = GraphicsDevice.Strategy.CurrentContext.Strategy.ToConcrete<ConcreteGraphicsContext>().D3dContext;

                return new D3D11.Buffer(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, bufferDesc);
            }
        }

        public override void PlatformContextLost()
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
